#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Games.Ring.Data;

namespace Games.Ring;

public record struct Score(int Perfect, int Good, int Missed, List<int> Combos)
{
    private bool _comboEnded;
    public int Total => Perfect + Good + Missed;

    public void AddPerfect()
    {
        Perfect++;
        RaiseCombo();
    }

    public void AddGood()
    {
        Good++;
        RaiseCombo();
    }

    public void AddMissed()
    {
        Missed++;
        EndCombo();
    }

    private void RaiseCombo()
    {
        if (_comboEnded)
        {
            _comboEnded = false;
            Combos.Add(1);
        }
        else
        {
            Combos[Combos.Count - 1]++;
        }
    }

    private void EndCombo() => _comboEnded = true;

    public int Calculate(Rules rules)
    {
        return (int)((Perfect * rules.PerfectValue
                      + Good * rules.GoodValue
                      + Missed * rules.MissedValue)
                     / MathF.Min(Total, 1)
                     * rules.MaxScore);
    }

    public void NormalizeCombos(Rules rules)
    {
        Combos = Combos.OrderByDescending(v => v).Take(rules.TopComboCount).ToList();
    }
}
