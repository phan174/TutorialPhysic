using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class RandomColor : MonoBehaviour
{
    // URP Lit / Unlit use "_BaseColor"; the Built-in pipeline and many older shaders use "_Color".
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private const string EmissionKeyword = "_EMISSION";

    [Tooltip("Pick a new random color when the scene starts.")]
    [SerializeField] private bool randomizeOnStart = true;

    [SerializeField] private bool randomizeBaseColor = true;

    [Tooltip("The material must already have Emission ticked in its Inspector.")]
    [SerializeField] private bool randomizeEmission = true;

    private Renderer rend;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (randomizeOnStart)
            Randomize();
    }

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        Material[] materials = rend.sharedMaterials;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material == null)
                continue;

            // One random hue per slot, so base color and glow match.
            Color random = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

            // Property block overrides colors for this renderer only; the material itself is untouched.
            rend.GetPropertyBlock(block, i);

            if (randomizeBaseColor)
                ApplyBaseColor(material, random);

            if (randomizeEmission)
                ApplyEmission(material, random);

            rend.SetPropertyBlock(block, i);
        }
    }

    [ContextMenu("Restore Original")]
    public void RestoreOriginal()
    {
        for (int i = 0; i < rend.sharedMaterials.Length; i++)
            rend.SetPropertyBlock(null, i);
    }

    private void ApplyBaseColor(Material material, Color random)
    {
        int colorId = material.HasProperty(BaseColorId) ? BaseColorId
                    : material.HasProperty(ColorId) ? ColorId
                    : -1;

        if (colorId == -1)
        {
            Debug.LogWarning($"{name}: material '{material.name}' has no _BaseColor or _Color property.", this);
            return;
        }

        Color original = material.GetColor(colorId);
        random.a = original.a;   // keep transparency exactly as it was
        block.SetColor(colorId, random);
    }

    private void ApplyEmission(Material material, Color random)
    {
        // A property block cannot switch emission on; that would mean changing the material.
        if (!material.HasProperty(EmissionColorId) || !material.IsKeywordEnabled(EmissionKeyword))
        {
            Debug.LogWarning($"{name}: Emission is off on material '{material.name}'. Tick 'Emission' on the material first.", this);
            return;
        }

        // Emission is an HDR color: keep the original brightness (intensity), change only the hue.
        Color original = material.GetColor(EmissionColorId);
        float intensity = Mathf.Max(original.r, original.g, original.b);
        if (intensity <= 0f)
            intensity = 1f;   // emission enabled but black: use normal brightness

        Color emission = random * intensity;
        emission.a = original.a;
        block.SetColor(EmissionColorId, emission);
    }
}
