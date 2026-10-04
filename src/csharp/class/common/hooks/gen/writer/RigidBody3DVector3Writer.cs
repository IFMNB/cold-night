using ColdNight.src.common.hooks;
using ColdNight.src.generated;
using Godot;

[GlobalClass] public partial class RigidBody3DVector3Writer : GenWriter<RigidBody3DVector3Properties>
{
    [Export] public new RigidBody3DVector3Properties PropertyWrite {get => base.PropertyWrite;set => base.PropertyWrite = value;}
}