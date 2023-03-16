using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

//=====================================================================================================================================================================================================

[Serializable, PostProcess(typeof(UPGEN_Lighting_Renderer), PostProcessEvent.BeforeTransparent, "Custom/UPGEN Lighting")]
public sealed class UPGEN_Lighting : PostProcessEffectSettings
{
    [Range(0f, 5f), Tooltip("Global effect intensity.")]
    public FloatParameter intensity = new FloatParameter { value = 1f };

    public override bool IsEnabledAndSupported(PostProcessRenderContext context) => enabled.value && intensity.value > 0f;
}

//=====================================================================================================================================================================================================

public sealed class UPGEN_Lighting_Renderer : PostProcessEffectRenderer<UPGEN_Lighting>
{
    private const string SHADER = "Hidden/Shader/UPGEN_Lighting";
    private static Shader _shader;

    public override void Render(PostProcessRenderContext context)
    {
        if (_shader == null) _shader = Shader.Find(SHADER);
        var sheet = context.propertySheets.Get(_shader);

        var cam = context.camera;
        sheet.properties.SetFloat("_Intensity", settings.intensity.value);
        UL_Renderer.SetupForCamera(cam, sheet.properties);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}

//=====================================================================================================================================================================================================