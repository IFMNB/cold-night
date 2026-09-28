using Godot;

namespace ColdNight.src.game;

/// <summary>
/// Очень краткий шорткат-обертка над операциями типа `RayCast3D` в пространстве объекта
/// </summary>
public readonly struct FastRayCast3D
{
    /// <summary>
    /// Если API все таки возвращает эту структуру, то смотрите это свойство
    /// 
    /// `false` - ни одно поле не будет инициализировано, так что можно выбрасывать это
    /// </summary>
    public readonly bool Hit;
    public readonly Vector3? Position;
    public readonly Vector3? Normal;
    /// <summary>
    /// Расстояние между `from` и `Position`
    /// </summary>
    public readonly float? Distance;
    public readonly ulong? ColliderId;
    public readonly int? FaceIndex;
    public readonly Rid? RID;
    public readonly int? Shape;
    public readonly CollisionObject3D? Collider;

    private FastRayCast3D(
        bool hit,
        Vector3? position = null,
        Vector3? normal = null,
        float? distance = null,
        ulong? colliderId = null,
        int? faceIndex = null,
        Rid? rid = null,
        int? shape = null,
        CollisionObject3D? collider = null)
    {
        Hit = hit;
        Position = position;
        Normal = normal;
        Distance = distance;
        ColliderId = colliderId;
        FaceIndex = faceIndex;
        RID = rid;
        Shape = shape;
        Collider = collider;
    }

    /// <summary>
    /// Прямое создание луча
    /// </summary>
    /// <param name="world"></param>
    /// <param name="from"></param>
    /// <param name="direction"></param>
    /// <param name="exclude"></param>
    /// <returns></returns>
    public static FastRayCast3D From(
        PhysicsDirectSpaceState3D world,
        Vector3 from,
        Vector3 direction,
        Godot.Collections.Array<Rid>? exclude = null)
    {
        var query = PhysicsRayQueryParameters3D.Create(from, from + direction);
        if (exclude != null)
            query.Exclude = exclude;

        return From(world, query);
    }

    /// <summary>
    /// Проход луча по уже заданным параметрам. Дает больше настроек, но вручную
    /// </summary>
    /// <param name="world"></param>
    /// <param name="queryParameters3D"></param>
    /// <returns></returns>
    public static FastRayCast3D From(PhysicsDirectSpaceState3D world, PhysicsRayQueryParameters3D queryParameters3D)
    {
        var result = world.IntersectRay(queryParameters3D);

        if (result.Count == 0)
            return new FastRayCast3D(false);

        var position = result["position"].AsVector3();

        return new FastRayCast3D(
            true,
            position,
            result["normal"].AsVector3(),
            queryParameters3D.From.DistanceTo(position),
            result["collider_id"].AsUInt64(),
            result["face_index"].AsInt32(),
            result["rid"].AsRid(),
            result["shape"].AsInt32(),
            result["collider"].AsGodotObject() as CollisionObject3D
        );
    }

    /// <summary>
    /// Возьмет данные из `<paramref name="ray"/>`
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    public static FastRayCast3D From(RayCast3D ray)
    {
        if (!ray.IsColliding())
            return new FastRayCast3D(false);

        var collider = ray.GetCollider() as CollisionObject3D;
        var position = ray.GetCollisionPoint();

        return new FastRayCast3D(
            true,
            position,
            ray.GetCollisionNormal(),
            ray.GlobalPosition.DistanceTo(position),
            collider?.GetInstanceId() ?? 0,
            ray.GetCollisionFaceIndex(),
            collider?.GetRid(),
            ray.GetColliderShape(),
            collider
        );
    }
}