using Godot;

namespace ColdNight.src.game.character.armature;

/// <summary>
/// Абстрактный класс, задающий параметризацию поиска имен и остальных частей костей в скелете.
/// Короче говоря, через него ищутся все нужные кости.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/brick_wall.svg")] public abstract partial class ArmatureBuilder : Resource
{
    [Export] public ArmatureMode Mode;

    public enum ArmatureMode
    {
        /// <summary>
        /// Не найдено нужной кости? Подойдет любая. Нет костей? Ошибка.
        /// </summary>
        Regular,

        /// <summary>
        /// Не найдено нужной кости? Подойдет любая. Нет костей? Мягко умолчим.
        /// </summary>
        Soft,
        
        /// <summary>
        /// Не найдено нужной кости? Ошибка.
        /// </summary>
        Strict
    }
}