using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 输入录制器
    /// 提供输入的录制、保存、加载和回放功能
    /// </summary>
    public class InputRecorder : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// 录制开始事件
        /// </summary>
        public event Action OnRecordingStarted;
        
        /// <summary>
        /// 录制停止事件
        /// </summary>
        public event Action<InputRecording> OnRecordingStopped;
        
        /// <summary>
        /// 回放开始事件
        /// </summary>
        public event Action<InputRecording> OnPlaybackStarted;
        
        /// <summary>
        /// 回放结束事件
        /// </summary>
        public event Action OnPlaybackEnded;
        
        /// <summary>
        /// 回放输入事件
        /// </summary>
        public event Action<Vector2> OnPlaybackInput;
        #endregion

        #region Properties
        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }
        
        /// <summary>
        /// 是否正在回放
        /// </summary>
        public bool IsPlaying { get; private set; }
        
        /// <summary>
        /// 当前录制时长
        /// </summary>
        public float RecordingDuration => IsRecording ? Time.time - recordingStartTime : 0f;
        
        /// <summary>
        /// 回放进度（0-1）
        /// </summary>
        public float PlaybackProgress 
        { 
            get 
            { 
                if (!IsPlaying || currentRecording == null || currentRecording.duration <= 0)
                    return 0f;
                return Mathf.Clamp01((Time.time - playbackStartTime) / currentRecording.duration);
            } 
        }
        #endregion

        #region Serialized Fields
        [Header("=== 录制设置 ===")]
        [SerializeField] private bool autoSaveRecordings = true;
        [SerializeField] private string recordingDirectory = "InputRecordings";
        [SerializeField] private int maxRecordingCount = 50;
        [SerializeField] private float maxRecordingDuration = 300f; // 5分钟

        [Header("=== 回放设置 ===")]
        [SerializeField] private bool loopPlayback = false;
        [SerializeField] private float playbackSpeed = 1f;
        [SerializeField] private bool simulateTimestamp = true;

        [Header("=== 性能优化 ===")]
        [SerializeField] private int inputBufferSize = 10000;
        [SerializeField] private float inputSampleRate = 60f; // 每秒采样率
        [SerializeField] private bool compressRecordings = true;

        [Header("=== 调试选项 ===")]
        [SerializeField] private bool showRecordingInfo = true;
        [SerializeField] private bool logRecordingEvents = false;
        #endregion

        #region Private Fields
        // 录制数据
        private List<InputFrame> currentFrames = new List<InputFrame>();
        private InputRecording currentRecording;
        private float recordingStartTime;
        private float lastInputTime;
        private int frameCounter;

        // 回放数据
        private float playbackStartTime;
        private int playbackFrameIndex;
        private bool playbackInitialized;

        // 配置
        private InputConfiguration config;

        // 文件系统
        private string recordingPath;
        private readonly Dictionary<string, InputRecording> loadedRecordings = new Dictionary<string, InputRecording>();

        // 性能优化
        private float inputSampleInterval;
        private Vector2 lastRecordedInput = Vector2.zero;
        private float lastSampleTime = 0f;

        // 对象池
        private readonly Queue<InputFrame> framePool = new Queue<InputFrame>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            InitializeFileSystem();
        }

        private void Update()
        {
            if (IsPlaying)
            {
                UpdatePlayback();
            }

            if (IsRecording && maxRecordingDuration > 0 && RecordingDuration >= maxRecordingDuration)
            {
                StopRecording();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 初始化录制器
        /// </summary>
        public void Initialize()
        {
            inputSampleInterval = 1f / inputSampleRate;
            currentFrames = new List<InputFrame>(inputBufferSize);
            
            // 预分配对象池
            for (int i = 0; i < 100; i++)
            {
                framePool.Enqueue(new InputFrame());
            }

            Debug.Log("[InputRecorder] 输入录制器初始化完成");
        }

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        private void InitializeFileSystem()
        {
            recordingPath = Path.Combine(Application.persistentDataPath, recordingDirectory);
            
            if (!Directory.Exists(recordingPath))
            {
                Directory.CreateDirectory(recordingPath);
                Debug.Log($"[InputRecorder] 创建录制目录: {recordingPath}");
            }

            // 清理过期的录制文件
            CleanupOldRecordings();
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        public void SetConfiguration(InputConfiguration newConfig)
        {
            config = newConfig;
            
            if (config != null)
            {
                inputSampleRate = config.inputUpdateRate;
                inputSampleInterval = 1f / inputSampleRate;
            }
        }
        #endregion

        #region Recording
        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecording()
        {
            if (IsRecording)
            {
                Debug.LogWarning("[InputRecorder] 已经在录制中");
                return;
            }

            if (IsPlaying)
            {
                StopPlayback();
            }

            // 重置录制状态
            currentFrames.Clear();
            recordingStartTime = Time.time;
            lastInputTime = Time.time;
            frameCounter = 0;
            lastRecordedInput = Vector2.zero;
            lastSampleTime = Time.time;

            IsRecording = true;

            Debug.Log("[InputRecorder] 开始录制输入");
            OnRecordingStarted?.Invoke();

            if (logRecordingEvents)
            {
                Debug.Log($"[InputRecorder] 录制开始 - 时间: {recordingStartTime}");
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public InputRecording StopRecording()
        {
            if (!IsRecording)
            {
                Debug.LogWarning("[InputRecorder] 当前没有在录制");
                return null;
            }

            IsRecording = false;
            float recordingDuration = Time.time - recordingStartTime;

            // 创建录制对象
            currentRecording = CreateRecording(recordingDuration);

            Debug.Log($"[InputRecorder] 录制结束 - 时长: {recordingDuration:F2}s, 帧数: {currentFrames.Count}");
            OnRecordingStopped?.Invoke(currentRecording);

            // 自动保存
            if (autoSaveRecordings)
            {
                string filename = $"Recording_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
                SaveRecording(currentRecording, filename);
            }

            if (logRecordingEvents)
            {
                Debug.Log($"[InputRecorder] 录制停止 - 帧数: {currentFrames.Count}, 时长: {recordingDuration:F2}s");
            }

            return currentRecording;
        }

        /// <summary>
        /// 录制移动输入
        /// </summary>
        public void RecordMovementInput(Vector2 inputVector)
        {
            if (!IsRecording) return;

            float currentTime = Time.time;
            
            // 采样率控制
            if (currentTime - lastSampleTime < inputSampleInterval) return;
            
            // 输入变化检测（可以减少冗余数据）
            if (Vector2.Distance(inputVector, lastRecordedInput) < 0.001f) return;

            RecordInputFrame(InputFrameType.Movement, inputVector, currentTime - recordingStartTime);
            
            lastRecordedInput = inputVector;
            lastSampleTime = currentTime;
        }

        /// <summary>
        /// 录制输入开始
        /// </summary>
        public void RecordInputStart()
        {
            if (!IsRecording) return;

            RecordInputFrame(InputFrameType.InputStart, Vector2.zero, Time.time - recordingStartTime);
        }

        /// <summary>
        /// 录制输入结束
        /// </summary>
        public void RecordInputEnd()
        {
            if (!IsRecording) return;

            RecordInputFrame(InputFrameType.InputEnd, Vector2.zero, Time.time - recordingStartTime);
        }

        /// <summary>
        /// 录制输入帧
        /// </summary>
        private void RecordInputFrame(InputFrameType frameType, Vector2 inputVector, float timestamp)
        {
            InputFrame frame = GetFrameFromPool();
            frame.frameType = frameType;
            frame.inputVector = inputVector;
            frame.timestamp = timestamp;
            frame.frameIndex = frameCounter++;

            currentFrames.Add(frame);

            // 限制缓冲区大小
            if (currentFrames.Count > inputBufferSize)
            {
                ReturnFrameToPool(currentFrames[0]);
                currentFrames.RemoveAt(0);
            }

            if (logRecordingEvents && frameType == InputFrameType.Movement)
            {
                Debug.Log($"[InputRecorder] 录制帧 {frame.frameIndex}: {inputVector} @ {timestamp:F3}s");
            }
        }

        /// <summary>
        /// 创建录制对象
        /// </summary>
        private InputRecording CreateRecording(float duration)
        {
            var recording = new InputRecording
            {
                id = Guid.NewGuid().ToString(),
                name = $"Recording_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                creationTime = DateTime.Now,
                duration = duration,
                frameCount = currentFrames.Count,
                frames = new InputFrame[currentFrames.Count]
            };

            // 复制帧数据
            for (int i = 0; i < currentFrames.Count; i++)
            {
                recording.frames[i] = new InputFrame
                {
                    frameType = currentFrames[i].frameType,
                    inputVector = currentFrames[i].inputVector,
                    timestamp = currentFrames[i].timestamp,
                    frameIndex = currentFrames[i].frameIndex
                };
            }

            return recording;
        }
        #endregion

        #region Playback
        /// <summary>
        /// 开始回放
        /// </summary>
        public void StartPlayback(InputRecording recording)
        {
            if (recording == null)
            {
                Debug.LogError("[InputRecorder] 录制数据为空");
                return;
            }

            if (IsRecording)
            {
                StopRecording();
            }

            currentRecording = recording;
            playbackStartTime = Time.time;
            playbackFrameIndex = 0;
            playbackInitialized = true;
            IsPlaying = true;

            Debug.Log($"[InputRecorder] 开始回放: {recording.name} (时长: {recording.duration:F2}s, 帧数: {recording.frameCount})");
            OnPlaybackStarted?.Invoke(recording);
        }

        /// <summary>
        /// 停止回放
        /// </summary>
        public void StopPlayback()
        {
            if (!IsPlaying) return;

            IsPlaying = false;
            playbackInitialized = false;
            playbackFrameIndex = 0;

            Debug.Log("[InputRecorder] 回放结束");
            OnPlaybackEnded?.Invoke();
        }

        /// <summary>
        /// 更新回放
        /// </summary>
        private void UpdatePlayback()
        {
            if (!IsPlaying || currentRecording == null || !playbackInitialized) return;

            float currentPlaybackTime = (Time.time - playbackStartTime) * playbackSpeed;

            // 处理当前时间点的所有帧
            while (playbackFrameIndex < currentRecording.frames.Length)
            {
                InputFrame frame = currentRecording.frames[playbackFrameIndex];
                
                if (frame.timestamp > currentPlaybackTime) break;

                ProcessPlaybackFrame(frame);
                playbackFrameIndex++;
            }

            // 检查回放是否结束
            if (playbackFrameIndex >= currentRecording.frames.Length)
            {
                if (loopPlayback)
                {
                    RestartPlayback();
                }
                else
                {
                    StopPlayback();
                }
            }
        }

        /// <summary>
        /// 处理回放帧
        /// </summary>
        private void ProcessPlaybackFrame(InputFrame frame)
        {
            switch (frame.frameType)
            {
                case InputFrameType.Movement:
                    OnPlaybackInput?.Invoke(frame.inputVector);
                    break;
                case InputFrameType.InputStart:
                    // 可以触发相应的开始事件
                    break;
                case InputFrameType.InputEnd:
                    // 可以触发相应的结束事件
                    break;
            }

            if (logRecordingEvents)
            {
                Debug.Log($"[InputRecorder] 回放帧 {frame.frameIndex}: {frame.inputVector} @ {frame.timestamp:F3}s");
            }
        }

        /// <summary>
        /// 重新开始回放
        /// </summary>
        private void RestartPlayback()
        {
            playbackFrameIndex = 0;
            playbackStartTime = Time.time;
            Debug.Log("[InputRecorder] 循环回放重新开始");
        }

        /// <summary>
        /// 设置回放速度
        /// </summary>
        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = Mathf.Clamp(speed, 0.1f, 10f);
        }

        /// <summary>
        /// 跳转到回放位置
        /// </summary>
        public void SeekPlayback(float normalizedTime)
        {
            if (!IsPlaying || currentRecording == null) return;

            normalizedTime = Mathf.Clamp01(normalizedTime);
            float targetTime = normalizedTime * currentRecording.duration;
            
            // 找到对应的帧索引
            playbackFrameIndex = 0;
            for (int i = 0; i < currentRecording.frames.Length; i++)
            {
                if (currentRecording.frames[i].timestamp > targetTime)
                {
                    playbackFrameIndex = i;
                    break;
                }
            }

            playbackStartTime = Time.time - targetTime / playbackSpeed;
        }
        #endregion

        #region File Operations
        /// <summary>
        /// 保存录制
        /// </summary>
        public bool SaveRecording(InputRecording recording, string filename)
        {
            if (recording == null || string.IsNullOrEmpty(filename))
            {
                Debug.LogError("[InputRecorder] 保存参数无效");
                return false;
            }

            try
            {
                string filePath = Path.Combine(recordingPath, filename + ".json");
                string jsonData = JsonUtility.ToJson(recording, true);

                if (compressRecordings)
                {
                    jsonData = CompressString(jsonData);
                }

                File.WriteAllText(filePath, jsonData);
                loadedRecordings[filename] = recording;

                Debug.Log($"[InputRecorder] 录制已保存: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputRecorder] 保存录制失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载录制
        /// </summary>
        public InputRecording LoadRecording(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogError("[InputRecorder] 文件名无效");
                return null;
            }

            // 检查缓存
            if (loadedRecordings.ContainsKey(filename))
            {
                return loadedRecordings[filename];
            }

            try
            {
                string filePath = Path.Combine(recordingPath, filename + ".json");
                
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"[InputRecorder] 录制文件不存在: {filePath}");
                    return null;
                }

                string jsonData = File.ReadAllText(filePath);

                if (compressRecordings)
                {
                    jsonData = DecompressString(jsonData);
                }

                InputRecording recording = JsonUtility.FromJson<InputRecording>(jsonData);
                loadedRecordings[filename] = recording;

                Debug.Log($"[InputRecorder] 录制已加载: {filename}");
                return recording;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputRecorder] 加载录制失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有录制文件名
        /// </summary>
        public string[] GetRecordingList()
        {
            try
            {
                if (!Directory.Exists(recordingPath))
                    return new string[0];

                string[] files = Directory.GetFiles(recordingPath, "*.json");
                string[] filenames = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    filenames[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                return filenames;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputRecorder] 获取录制列表失败: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// 删除录制
        /// </summary>
        public bool DeleteRecording(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return false;

            try
            {
                string filePath = Path.Combine(recordingPath, filename + ".json");
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    loadedRecordings.Remove(filename);
                    Debug.Log($"[InputRecorder] 录制已删除: {filename}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputRecorder] 删除录制失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清理过期录制
        /// </summary>
        private void CleanupOldRecordings()
        {
            try
            {
                string[] files = Directory.GetFiles(recordingPath, "*.json");
                
                if (files.Length > maxRecordingCount)
                {
                    // 按创建时间排序
                    Array.Sort(files, (x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));
                    
                    // 删除最老的文件
                    int filesToDelete = files.Length - maxRecordingCount;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        File.Delete(files[i]);
                        Debug.Log($"[InputRecorder] 清理过期录制: {Path.GetFileName(files[i])}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputRecorder] 清理过期录制失败: {ex.Message}");
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 从对象池获取帧
        /// </summary>
        private InputFrame GetFrameFromPool()
        {
            return framePool.Count > 0 ? framePool.Dequeue() : new InputFrame();
        }

        /// <summary>
        /// 返回帧到对象池
        /// </summary>
        private void ReturnFrameToPool(InputFrame frame)
        {
            frame.Reset();
            if (framePool.Count < 200) // 限制池大小
            {
                framePool.Enqueue(frame);
            }
        }

        /// <summary>
        /// 压缩字符串（简单实现）
        /// </summary>
        private string CompressString(string text)
        {
            // 这里可以实现更复杂的压缩算法
            return text;
        }

        /// <summary>
        /// 解压字符串
        /// </summary>
        private string DecompressString(string compressedText)
        {
            return compressedText;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            if (IsRecording)
            {
                StopRecording();
            }

            if (IsPlaying)
            {
                StopPlayback();
            }

            // 清理事件
            OnRecordingStarted = null;
            OnRecordingStopped = null;
            OnPlaybackStarted = null;
            OnPlaybackEnded = null;
            OnPlaybackInput = null;

            // 清理数据
            currentFrames?.Clear();
            loadedRecordings.Clear();
            
            // 清理对象池
            framePool.Clear();
        }
        #endregion

        #region Public API
        /// <summary>
        /// 获取录制统计信息
        /// </summary>
        public RecorderStats GetRecorderStats()
        {
            return new RecorderStats
            {
                isRecording = IsRecording,
                isPlaying = IsPlaying,
                recordingDuration = RecordingDuration,
                playbackProgress = PlaybackProgress,
                currentFrameCount = currentFrames?.Count ?? 0,
                loadedRecordingCount = loadedRecordings.Count,
                maxRecordingDuration = maxRecordingDuration,
                playbackSpeed = playbackSpeed
            };
        }

        /// <summary>
        /// 切换录制状态
        /// </summary>
        public void ToggleRecording()
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }
        #endregion

        #region Debugging
        private void OnGUI()
        {
            if (showRecordingInfo && (IsRecording || IsPlaying))
            {
                DrawRecordingInfo();
            }
        }

        private void DrawRecordingInfo()
        {
            GUILayout.BeginArea(new Rect(640, 10, 300, 150));
            GUILayout.Label("=== 录制器状态 ===");
            
            if (IsRecording)
            {
                GUILayout.Label($"正在录制... {RecordingDuration:F1}s");
                GUILayout.Label($"帧数: {currentFrames.Count}");
            }
            
            if (IsPlaying && currentRecording != null)
            {
                GUILayout.Label($"正在回放: {currentRecording.name}");
                GUILayout.Label($"进度: {(PlaybackProgress * 100):F1}%");
                GUILayout.Label($"速度: {playbackSpeed}x");
            }
            
            GUILayout.Label($"已加载录制: {loadedRecordings.Count}");
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Data Structures
    /// <summary>
    /// 输入帧类型
    /// </summary>
    public enum InputFrameType
    {
        Movement,
        InputStart,
        InputEnd
    }

    /// <summary>
    /// 输入帧数据
    /// </summary>
    [Serializable]
    public class InputFrame
    {
        public InputFrameType frameType;
        public Vector2 inputVector;
        public float timestamp;
        public int frameIndex;

        public void Reset()
        {
            frameType = InputFrameType.Movement;
            inputVector = Vector2.zero;
            timestamp = 0f;
            frameIndex = 0;
        }
    }

    /// <summary>
    /// 输入录制数据
    /// </summary>
    [Serializable]
    public class InputRecording
    {
        public string id;
        public string name;
        public DateTime creationTime;
        public float duration;
        public int frameCount;
        public InputFrame[] frames;
    }

    /// <summary>
    /// 录制器统计信息
    /// </summary>
    public struct RecorderStats
    {
        public bool isRecording;
        public bool isPlaying;
        public float recordingDuration;
        public float playbackProgress;
        public int currentFrameCount;
        public int loadedRecordingCount;
        public float maxRecordingDuration;
        public float playbackSpeed;
    }
    #endregion
} 