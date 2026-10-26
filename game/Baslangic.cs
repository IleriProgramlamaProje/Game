using Godot;

public partial class Baslangic : Node
{
	public override void _Ready()
	{
		GD.Print("C# ortamı hazır — Godot ", Engine.GetVersionInfo()["string"]);
	}
}
