using UnityEngine;

public interface IClickable
{
    bool CanInteract { get; }
    void OnClick(Vector3 clickPosition);
    float GetInteractionRange();
} 