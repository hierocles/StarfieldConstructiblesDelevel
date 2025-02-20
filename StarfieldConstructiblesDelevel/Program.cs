using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace StarfieldConstructiblesDelevel
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IStarfieldMod, IStarfieldModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Starfield, "DeleveledConstructibles.esm")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IStarfieldMod, IStarfieldModGetter> state)
        {
            foreach (var cobjGetter in state.LoadOrder.PriorityOrder.ConstructibleObject().WinningOverrides())
            {
                if (cobjGetter.Conditions.Count == 0)
                {
                    continue;
                }

                foreach (var cobjGetterCondition in cobjGetter.Conditions)
                {
                    if (cobjGetterCondition is not { Data: IGetLevelConditionDataGetter refData } ||
                        refData.Reference.FormKey != FormKey.Factory("000014:Starfield.esm")) continue;
                    var condFloat = (IConditionFloatGetter)cobjGetterCondition;
                    if (condFloat.ComparisonValue.EqualsWithin(1) ||
                        (condFloat.CompareOperator != CompareOperator.GreaterThan &&
                         condFloat.CompareOperator != CompareOperator.GreaterThanOrEqualTo)) continue;
                    var cobj = state.PatchMod.ConstructibleObjects.GetOrAddAsOverride(cobjGetter);
                    var index = cobjGetter.Conditions.IndexOf(cobjGetterCondition);
                    ((IConditionFloat)cobj.Conditions[index]).ComparisonValue = 1;
                }
            }
        }
    }
}

