using ColdNight.src.generated;
using ColdNight.src.common.hooks;
using Godot;

[GlobalClass] public partial class Node3DVector3Reader : GenReader<Node3DVector3Properties>
{
    [Export] public new Node3DVector3Properties PropertyRead {get => base.PropertyRead;set => base.PropertyRead = value;}
}