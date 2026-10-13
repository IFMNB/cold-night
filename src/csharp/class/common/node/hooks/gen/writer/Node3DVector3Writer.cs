using ColdNight.src.common.hooks;
using ColdNight.src.generated;
using Godot;

[GlobalClass] public partial class Node3DVector3Writer : GenWriter<Node3DVector3Properties>
{
    [Export] public new Node3DVector3Properties PropertyWrite {get => base.PropertyWrite;set => base.PropertyWrite = value;}
}