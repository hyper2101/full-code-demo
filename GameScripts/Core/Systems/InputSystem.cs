using UnityEngine;

public class InputSystem
{
    private WorldManager _world;

    public Vector3 mouseWorldPosition;
    public Vector3 grabOffset;
    public Draggable HoveredDraggable;
    public Draggable DraggingDraggable;
    public Interactable HoveredInteractable;
    public Hoverable CurrentHoverable;

    public InputSystem(WorldManager world)
    {
        _world = world;
    }

    public GameCard DraggingCard => DraggingDraggable as GameCard;
    public GameCard HoveredCard => HoveredDraggable as GameCard;
}
