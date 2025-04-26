// ElementalEffect.cs
// Determines rock-paper-scissors damage multipliers.
public static class ElementalEffect
{
    /// <summary>
    /// Returns the damage multiplier of an attacker element vs. a defender element.
    /// Fire→Ice, Ice→Water, Water→Fire each deal 1.5× damage; all other combos 1×.
    /// </summary>
    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        if (attacker == ElementType.Fire && defender == ElementType.Water) return 0.5f;
        if (attacker == ElementType.Ice && defender == ElementType.Fire) return 0.5f;
        if (attacker == ElementType.Water && defender == ElementType.Ice) return 0.5f;

        if (attacker == ElementType.Fire && defender == ElementType.Ice) return 1.5f;
        if (attacker == ElementType.Ice && defender == ElementType.Water) return 1.5f;
        if (attacker == ElementType.Water && defender == ElementType.Fire) return 1.5f;
        return 1f;
    }
}
