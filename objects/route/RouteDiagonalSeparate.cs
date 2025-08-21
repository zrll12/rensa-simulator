using Godot;

namespace RensaSimulator.objects.route;

public partial class RouteDiagonalSeparate : Node2D {
    [Export]
    public Color Route1Color {
        get => _spriteMaterial.GetShaderParameter("to1").AsColor();
        set => _spriteMaterial.SetShaderParameter("to1", value);
    }

    [Export]
    public Color Route2Color {
        get => _spriteMaterial.GetShaderParameter("to2").AsColor();
        set => _spriteMaterial.SetShaderParameter("to2", value);
    }

    private ShaderMaterial _spriteMaterial = new();

    public override void _Ready() {
        _spriteMaterial.Shader = GD.Load<Shader>("res://shader/double_color_replace.gdshader");
        _spriteMaterial.SetShaderParameter("from1", new Color("#1c499f"));
        _spriteMaterial.SetShaderParameter("from2", new Color("#03ff00"));

        GetNode<Sprite2D>("Sprite2D").Material = _spriteMaterial;
    }
}