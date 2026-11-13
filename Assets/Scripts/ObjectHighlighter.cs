using System.Collections.Generic;
using UnityEngine;

// Tints every renderer of an object and restores it exactly, without touching the materials.
public sealed class ObjectHighlighter
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private sealed class Slot
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public int ColorId;
        public readonly MaterialPropertyBlock Saved = new MaterialPropertyBlock();
    }

    private readonly List<Slot> slots = new List<Slot>();
    private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    private readonly Color color;

    public bool IsOn { get; private set; }

    public ObjectHighlighter(GameObject target, Color color)
    {
        this.color = color;

        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                int colorId = FindColorProperty(materials[i]);
                if (colorId != -1)
                    slots.Add(new Slot { Renderer = renderer, MaterialIndex = i, ColorId = colorId });
            }
        }
    }

    public void Show()
    {
        if (IsOn)
            return;

        foreach (Slot slot in slots)
        {
            slot.Renderer.GetPropertyBlock(slot.Saved, slot.MaterialIndex);
            slot.Renderer.GetPropertyBlock(block, slot.MaterialIndex);
            block.SetColor(slot.ColorId, color);
            slot.Renderer.SetPropertyBlock(block, slot.MaterialIndex);
        }

        IsOn = true;
    }

    public void Hide()
    {
        if (!IsOn)
            return;

        foreach (Slot slot in slots)
            slot.Renderer.SetPropertyBlock(slot.Saved, slot.MaterialIndex);

        IsOn = false;
    }

    private static int FindColorProperty(Material material)
    {
        if (material == null) return -1;
        if (material.HasProperty(BaseColorId)) return BaseColorId;
        if (material.HasProperty(ColorId)) return ColorId;
        return -1;
    }
}
