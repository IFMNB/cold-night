using Godot;

namespace ColdNight.src;

/// <summary>
/// Статический класс для хранения утилитарных функций работы с <see cref="Variant"/> и его моделью.
/// 
/// <para>
/// <see cref="Variant"/> - это обертка над любых <see cref="Godot"/> совместимым значением, а значит
/// она при назначении на любой объект существует как дополнительная прослойка. Там где вы ожидаете
/// <see langword="null"/> вы можете получить <see cref="Variant"/> и наоборот. Некоторые типы, например
/// buildin структуры типа <see cref="Vector3"/> <see cref="Godot"/> сам приводит к <see langword="default"/>
/// если тот же csharp ожидал там <see langword="null"/>.
/// </para>
/// </summary>
public static class VariantExtension
{

    public static bool IsNull (Variant value) => value.VariantType == Variant.Type.Nil;
    public static bool TypeEqual (Variant left, Variant right) => left.VariantType == right.VariantType;

    public static bool ValueEqual (Variant left, Variant right)
    {
        if (left.VariantType != right.VariantType) return false;

        switch (left.VariantType)
        {
            case Variant.Type.Int: return left.AsInt64() == right.AsInt64();
            case Variant.Type.Float: return left.AsDouble() == right.AsDouble();
            case Variant.Type.String: return left.AsString() == right.AsString();
            case Variant.Type.Vector2: return left.AsVector2() == right.AsVector2();
            case Variant.Type.Vector3: return left.AsVector3() == right.AsVector3();
            case Variant.Type.Vector4: return left.AsVector4() == right.AsVector4();
            case Variant.Type.Color: return left.AsColor() == right.AsColor();
            case Variant.Type.Quaternion: return left.AsQuaternion() == right.AsQuaternion();
            case Variant.Type.Basis: return left.AsBasis() == right.AsBasis();
            case Variant.Type.Transform2D: return left.AsTransform2D() == right.AsTransform2D();
            case Variant.Type.Transform3D: return left.AsTransform3D() == right.AsTransform3D();
            case Variant.Type.Projection: return left.AsProjection() == right.AsProjection();
            default:
                #if DEBUG
                    GD.PushWarning ($"cannot find needle operation to do work with {left.VariantType} , {right.VariantType}");
                #endif
                return false;
        }
    }

    public static bool TryApply(Variant left, Variant right, OperationMode mode, out Variant result)
    {
        result = default;

        if (mode == OperationMode.Replace)
        {
            result = right;
            return true;
        }

        var lt = left.VariantType;
        var rt = right.VariantType;

        // ── (Int → Int || Float) ───────────────────────────
        if (IsNumeric(lt) && IsNumeric(rt))
        {
            if (mode == OperationMode.Divide && right.AsDouble() == 0.0)
                return false;

            if (lt == Variant.Type.Int && rt == Variant.Type.Int)
            {
                long l = left.AsInt64();
                long r = right.AsInt64();
                result = mode switch
                {
                    OperationMode.Add    => l + r,
                    OperationMode.Sub    => l - r,
                    OperationMode.Mult   => l * r,
                    OperationMode.Divide => l / r,
                    _                 => l,
                };
                return true;
            }

            double dl = left.AsDouble();
            double dr = right.AsDouble();
            result = mode switch
            {
                OperationMode.Add    => dl + dr,
                OperationMode.Sub    => dl - dr,
                OperationMode.Mult   => dl * dr,
                OperationMode.Divide => dl / dr,
                _                 => dl,
            };


            return true;
        }

        // ── String (Add = concat) ───────────────────────
        if (mode == OperationMode.Add && lt == Variant.Type.String && rt == Variant.Type.String)
        {
            string a = left.AsString();
            string b = right.AsString();
            result = a + b;
            return true;
        }

        // ── Vector2 ─────────────────────────────────────────────────
        if (lt == Variant.Type.Vector2 && rt == Variant.Type.Vector2)
        {
            Vector2 a = left.AsVector2();
            Vector2 b = right.AsVector2();
            if (mode == OperationMode.Divide && (b.X == 0f || b.Y == 0f)) return false;
            switch (mode)
            {
                case OperationMode.Add:    result = a + b; return true;
                case OperationMode.Sub:    result = a - b; return true;
                case OperationMode.Mult:   result = a * b; return true;
                case OperationMode.Divide: result = a / b; return true; 
            }
            return false;
        }
        if (lt == Variant.Type.Vector2 && IsNumeric(rt))
        {
            Vector2 a = left.AsVector2();
            float s = (float)right.AsDouble();
            switch (mode)
            {
                case OperationMode.Mult:   result = a * s; return true;
                case OperationMode.Divide: if (s == 0f) return false; result = a / s; return true;
            }
            return false;
        }
        if (IsNumeric(lt) && rt == Variant.Type.Vector2 && mode == OperationMode.Mult)
        {
            float s = (float)left.AsDouble();
            Vector2 b = right.AsVector2();
            result = b * s;
            return true;
        }

        // ── Vector3 ─────────────────────────────────────────────────
        if (lt == Variant.Type.Vector3 && rt == Variant.Type.Vector3)
        {
            Vector3 a = left.AsVector3();
            Vector3 b = right.AsVector3();
            if (mode == OperationMode.Divide && (b.X == 0f || b.Y == 0f || b.Z == 0f)) return false;
            switch (mode)
            {
                case OperationMode.Add:    result = a + b; return true;
                case OperationMode.Sub:    result = a - b; return true;
                case OperationMode.Mult:   result = a * b; return true;
                case OperationMode.Divide: result = a / b; return true;
            }
            return false;
        }
        if (lt == Variant.Type.Vector3 && IsNumeric(rt))
        {
            Vector3 a = left.AsVector3();
            float s = (float)right.AsDouble();
            switch (mode)
            {
                case OperationMode.Mult:   result = a * s; return true;
                case OperationMode.Divide: if (s == 0f) return false; result = a / s; return true;
            }
            return false;
        }
        if (IsNumeric(lt) && rt == Variant.Type.Vector3 && mode == OperationMode.Mult)
        {
            float s = (float)left.AsDouble();
            Vector3 b = right.AsVector3();
            result = b * s;
            return true;
        }

        // ── Vector4 ─────────────────────────────────────────────────
        if (lt == Variant.Type.Vector4 && rt == Variant.Type.Vector4)
        {
            Vector4 a = left.AsVector4();
            Vector4 b = right.AsVector4();
            if (mode == OperationMode.Divide
                && (b.X == 0f || b.Y == 0f || b.Z == 0f || b.W == 0f)) return false;
            switch (mode)
            {
                case OperationMode.Add:    result = a + b; return true;
                case OperationMode.Sub:    result = a - b; return true;
                case OperationMode.Mult:   result = a * b; return true;
                case OperationMode.Divide: result = a / b; return true;
            }
            return false;
        }
        if (lt == Variant.Type.Vector4 && IsNumeric(rt))
        {
            Vector4 a = left.AsVector4();
            float s = (float)right.AsDouble();
            switch (mode)
            {
                case OperationMode.Mult:   result = a * s; return true;
                case OperationMode.Divide: if (s == 0f) return false; result = a / s; return true;
            }
            return false;
        }
        if (IsNumeric(lt) && rt == Variant.Type.Vector4 && mode == OperationMode.Mult)
        {
            float s = (float)left.AsDouble();
            Vector4 b = right.AsVector4();
            result = b * s;
            return true;
        }

        // ── Color ───────────────────────────────────────────────────
        if (lt == Variant.Type.Color && rt == Variant.Type.Color)
        {
            Color a = left.AsColor();
            Color b = right.AsColor();
            if (mode == OperationMode.Divide
                && (b.R == 0f || b.G == 0f || b.B == 0f || b.A == 0f)) return false;
            switch (mode)
            {
                case OperationMode.Add:    result = a + b; return true;
                case OperationMode.Sub:    result = a - b; return true;
                case OperationMode.Mult:   result = a * b; return true;
                case OperationMode.Divide: result = a / b; return true;
            }
            return false;
        }
        if (lt == Variant.Type.Color && IsNumeric(rt))
        {
            Color a = left.AsColor();
            float s = (float)right.AsDouble();
            switch (mode)
            {
                case OperationMode.Mult:   result = a * s; return true;
                case OperationMode.Divide: if (s == 0f) return false; result = a / s; return true;
            }
            return false;
        }
        if (IsNumeric(lt) && rt == Variant.Type.Color && mode == OperationMode.Mult)
        {
            float s = (float)left.AsDouble();
            Color b = right.AsColor();
            result = b * s;
            return true;
        }

        // ── Quaternion ──────────────────────────────────────────────
        if (lt == Variant.Type.Quaternion && rt == Variant.Type.Quaternion && mode == OperationMode.Mult)
        {
            Quaternion a = left.AsQuaternion();
            Quaternion b = right.AsQuaternion();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Quaternion && rt == Variant.Type.Vector3 && mode == OperationMode.Mult)
        {
            Quaternion a = left.AsQuaternion();
            Vector3 b = right.AsVector3();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Quaternion && IsNumeric(rt) && mode == OperationMode.Mult)
        {
            Quaternion a = left.AsQuaternion();
            float s = (float)right.AsDouble();
            result = a * s;
            return true;
        }
        if (IsNumeric(lt) && rt == Variant.Type.Quaternion && mode == OperationMode.Mult)
        {
            float s = (float)left.AsDouble();
            Quaternion b = right.AsQuaternion();
            result = b * s;
            return true;
        }

        // ── Basis ───────────────────────────────────────────────────
        if (lt == Variant.Type.Basis && rt == Variant.Type.Basis && mode == OperationMode.Mult)
        {
            Basis a = left.AsBasis();
            Basis b = right.AsBasis();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Basis && rt == Variant.Type.Vector3 && mode == OperationMode.Mult)
        {
            Basis a = left.AsBasis();
            Vector3 b = right.AsVector3();
            result = a * b;
            return true;
        }

        // ── Transform2D ─────────────────────────────────────────────
        if (lt == Variant.Type.Transform2D && rt == Variant.Type.Transform2D && mode == OperationMode.Mult)
        {
            Transform2D a = left.AsTransform2D();
            Transform2D b = right.AsTransform2D();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Transform2D && rt == Variant.Type.Vector2 && mode == OperationMode.Mult)
        {
            Transform2D a = left.AsTransform2D();
            Vector2 b = right.AsVector2();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Transform2D && IsNumeric(rt) && mode == OperationMode.Mult)
        {
            Transform2D a = left.AsTransform2D();
            float s = (float)right.AsDouble();

            result = a.Scaled(Vector2.One * s);

            return true;
        }

        // ── Transform3D ─────────────────────────────────────────────
        if (lt == Variant.Type.Transform3D && rt == Variant.Type.Transform3D && mode == OperationMode.Mult)
        {
            Transform3D a = left.AsTransform3D();
            Transform3D b = right.AsTransform3D();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Transform3D && rt == Variant.Type.Vector3 && mode == OperationMode.Mult)
        {
            Transform3D a = left.AsTransform3D();
            Vector3 b = right.AsVector3();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Transform3D && IsNumeric(rt) && mode == OperationMode.Mult)
        {
            Transform3D a = left.AsTransform3D();
            float s = (float)right.AsDouble();

            result = a.Scaled(Vector3.One * s);

            return true;
        }

        // ── Projection ──────────────────────────────────────────────
        if (lt == Variant.Type.Projection && rt == Variant.Type.Projection && mode == OperationMode.Mult)
        {
            Projection a = left.AsProjection();
            Projection b = right.AsProjection();
            result = a * b;
            return true;
        }
        if (lt == Variant.Type.Projection && rt == Variant.Type.Vector4 && mode == OperationMode.Mult)
        {
            Projection a = left.AsProjection();
            Vector4 b = right.AsVector4();
            result = a * b;
            return true;
        }

        if (lt == Variant.Type.Projection && IsNumeric(rt) && mode == OperationMode.Mult)
        {
            Projection a = left.AsProjection();
            float s = (float)right.AsDouble();
            result = new Projection(
                a.X * s,
                a.Y * s,
                a.Z * s,
                a.W * s
            );
            return true;
        }



        #if DEBUG
            GD.PushWarning ($"cannot find needle operation to do work with {left.VariantType} , {right.VariantType}");
        #endif

        return false;
    }

    public static bool TryApply<[MustBeVariant] T> (Variant left, Variant right, OperationMode mode, out T answer)
    {
        answer = default!;

        if (TryApply(left, right, mode, out var apply_result))
            if (TryCast<T>(apply_result, out var cast_result))
            {
                answer = cast_result;
                return true;
            }

        return false;
    }

    public static bool TryCast<[MustBeVariant] T>(Variant value, out T result)
    {
        if (value.VariantType == Variant.Type.Nil)
        {
            result = default!;
            return false;
        }

        try
        {
            result = value.As<T>();
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
    }

    public static bool IsNumeric(Variant.Type t) =>
        t is Variant.Type.Int or Variant.Type.Float;
}

public enum OperationMode
{
    Add,
    Sub,
    Mult,
    Divide,
    Replace
}