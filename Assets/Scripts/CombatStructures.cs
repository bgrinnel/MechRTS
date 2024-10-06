/// Note from Jeff:
/// This is a work-in-progress, only making necessary elements of this right now.
/// hoping it will allow us a lot of flexibilty in the future (also, I can steal this code of mine for elsewhere) <summary>

public enum EPropertyType : byte
{
    Damage = 1,
    Health = 2,
}

public enum EPropertyEffect : byte
{
    Add = 1,
    Min = 2,
    Max = 4,
    Mod = 8,
}

public struct TPropertyModifier
{

}

public struct TPropertyContainer
{
    public float Max;
    public float Min;
    public float Current;

    public TPropertyContainer(float max)
    {
        Max = max;
        Current = max;
        Min = 0f;
    }

    public TPropertyContainer(float max, float min, float curr)
    {
        Max = max;
        Min = min;
        Current = curr;
    }

}

public struct TCombatContext
{
    public TPropertyContainer Damage;

    static public TCombatContext BasicAttack(float Damage)
    {
        return new TCombatContext{
            Damage = new TPropertyContainer{Max=float.MaxValue, Current=Damage, Min=float.MinValue}
        };
    }
}