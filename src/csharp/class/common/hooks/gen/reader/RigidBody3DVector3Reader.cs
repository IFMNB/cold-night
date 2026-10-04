using ColdNight.src.generated;
using ColdNight.src.common.hooks;
using Godot;

[GlobalClass] public partial class RigidBody3DVector3Reader : GenReader<RigidBody3DVector3Properties>
{
    [Export] public new RigidBody3DVector3Properties PropertyRead {get => base.PropertyRead;set => base.PropertyRead = value;}
}