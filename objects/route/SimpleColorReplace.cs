using Godot;

namespace RensaSimulator.objects.route;

public partial class SimpleColorReplace : Node2D {
    [Export]
    public Color RouteColor {
        get => _spriteMaterial.GetShaderParameter("to").AsColor();
        set => _spriteMaterial.SetShaderParameter("to", value);
    }

    private ShaderMaterial _spriteMaterial = new();

    public override void _Ready() {
        _spriteMaterial.Shader = GD.Load<Shader>("res://shader/color_replace.gdshader");
        _spriteMaterial.SetShaderParameter("from", new Color("#1c499f"));

        GetNode<Sprite2D>("Sprite2D").Material = _spriteMaterial;
    }
}