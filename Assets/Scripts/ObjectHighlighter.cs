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
        public Material Material;
        public int MaterialIndex;
        public int ColorId;
        public readonly MaterialPropertyBlock Saved = new MaterialPropertyBlock();
    }

    private readonly List<Slot> slots = new List<Slot>();
    private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    private readonly Color color;
    private readonly float strength;

    public bool IsOn { get; private set; }

    // strength: 0 = no change, 1 = fully replaced by the highlight color.
    public ObjectHighlighter(GameObject target, Color color, float strength)
    {
        this.color = color;
        this.strength = Mathf.Clamp01(strength);

        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                int colorId = FindColorProperty(materials[i]);
                if (colorId != -1)
                    slots.Add(new Slot { Renderer = renderer, Material = materials[i], MaterialIndex = i, ColorId = colorId });
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

            Color current = slot.Saved.HasColor(slot.ColorId)
                ? slot.Saved.GetColor(slot.ColorId)
                : slot.Material.GetColor(slot.ColorId);

            Color tinted = Color.Lerp(current, color, strength);
            tinted.a = current.a;

            slot.Renderer.GetPropertyBlock(block, slot.MaterialIndex);
            block.SetColor(slot.ColorId, tinted);
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
