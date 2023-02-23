namespace ScratchCardAsset.Core
{
    public enum Quality
    {
        Low = 4,
        Medium = 2,
        High = 1
    }

    public enum ScratchMode
    {
        Erase,
        Restore
    }

    public enum ScratchCardRenderType
    {
        MeshRenderer,
        SpriteRenderer,
        CanvasRenderer
    }

    public enum ScratchAnimationSpace
    {
        UV,
        Texture
    }

    public enum ProgressAccuracy
    {
        Default,
        High
    }
}