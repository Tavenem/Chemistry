using Tavenem.Chemistry.Elements;
using System.Linq;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Extension methods related to <see cref="ISubstance"/>.
    /// </summary>
    public static class SubstanceExtensions
    {
        /// <summary>
        /// Gets the proportion of this substance which is composed of pure water.
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <returns>The proportion of this substance which is composed of pure water, as a value
        /// between 0 and 1.</returns>
        public static decimal GetWaterProportion(this ISubstance substance)
        {
            if (substance is Chemical)
            {
                return substance.Equals(Substances.All.Water)
                    ? 1
                    : 0;
            }
            else
            {
                var waterProportion = 0.0m;
                foreach (var constituent in substance.Constituents)
                {
                    if (constituent.Key.Homogeneous is Chemical)
                    {
                        if (constituent.Key.Equals(Substances.All.Water))
                        {
                            waterProportion += constituent.Value;
                        }
                    }
                    else
                    {
                        waterProportion += GetWaterProportion(constituent.Key.Homogeneous) * constituent.Value;
                    }
                }
                return waterProportion;
            }
        }

        /// <summary>
        /// Indicates whether the given substance is entirely composed of the element Carbon.
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref name="substance"/> is entirely composed
        /// of the element Carbon; otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// This will return <see langword="true"/> for e.g. amorphous carbon, diamond, and any
        /// compound which contains no other elements.
        /// </remarks>
        public static bool IsCarbon(this ISubstance substance)
        {
            if (substance is Chemical chemical)
            {
                var elements = chemical.Formula.Elements.ToList();
                return elements.Count == 1 && elements[0].AtomicNumber == 6;
            }
            else
            {
                return substance.Constituents.All(x => x.Key.Homogeneous.IsCarbon());
            }
        }

        /// <summary>
        /// Indicates whether the given substance is a hydrocarbon.
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref name="substance"/> is a hydrocarbon;
        /// otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>
        /// The test returns <see langword="true"/> if the <paramref name="substance"/> contains at
        /// least 25% hydrocarbons, and if the proportion of hydrocarbons among the constituents of
        /// the substance which are not either pure carbon or water is at least 75%.
        /// </para>
        /// <para>
        /// For example, a substance which contains 50% pure carbon, 32% hydrocarbons, 10% water,
        /// and 8% minerals would qualify. In this case, 0.32 (the proportion of hydrocarbons in the
        /// mixture) is 80% of 0.4 (the proportion of material which is neither pure carbon nor
        /// water). Since 32% is greater than the 25% absolute minimum, and greater than the 75%
        /// threshhold of non-carbon, non-water constituents, the substance qualifies.
        /// </para>
        /// <para>
        /// Note: only pure hydrocarbons (containing only the elements Hydrogen and Carbon) are
        /// counted towards the required proportions. Hydrocarbons modeled as a single chemical
        /// formula containing other elements (such as Sulfur, for instance) do not count. If the
        /// impure hydrocarbon substance is modeled as a mixture of one or more pure hydrocarbons
        /// with other chemical impurities in solution, however, the proportions will be correctly
        /// calculated for that substance.
        /// </para>
        /// </remarks>
        public static bool IsHydrocarbon(this ISubstance substance)
        {
            var (carbonProportion, hydrocarbonProportion, waterProportion) = GetHydrocarbonProportions(substance);
            return hydrocarbonProportion >= 0.25m
                && hydrocarbonProportion >= 0.75m - (carbonProportion + waterProportion);
        }

        /// <summary>
        /// Indicates whether the given substance is a metal ore.
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref name="substance"/> is an ore; otherwise
        /// <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>
        /// The test is not perfect, since what constitutes an "ore" is a matter more of convention
        /// based on value than any rigorous definition. The method returns <see langword="true"/>
        /// if the <paramref name="substance"/> contains one or more metal elements, and possibly
        /// also hydrogen, oxygen, sulfur, and/or arsenic, but no other nonmetallic elements (i.e.
        /// is likely to be a metal oxide, sulfide, or arsenide). This is likely to include most
        /// common, important ore minerals, and exclude most non-ore minerals which contain metallic
        /// elements.
        /// </para>
        /// <para>
        /// In addition, the alkali and alkaline earth elements are not counted as "metals" for the
        /// purpose of this selection process, as these elements are not typically considered to
        /// constitute "ore" in the colloquial understanding. These elements also count as
        /// "nonmetals" for the purpose of excluding minerals which contain elements not counted as
        /// constituting "ore" according to the definition above.
        /// </para>
        /// <para>
        /// If the substance is a mixture, it is classified as an ore if at least 50% of its
        /// constituents are ores. Bauxite, for example, contains a variety of minerals which
        /// themselves fit the above definition of ores, along with other "filler" materials which
        /// do not. The proportion of ores is high compared to the "filler," however, so the mixture
        /// as a whole satisfies the criteria.
        /// </para>
        /// </remarks>
        public static bool IsMetalOre(this ISubstance substance)
        {
            var hasMetal = false;
            if (substance is IHomogeneous)
            {
                foreach (var element in substance
                    .GetChemicalConstituents()
                    .SelectMany(x => x.Formula.Elements))
                {
                    // H, O, S, As do not count against the substance, but do not make it an ore on
                    // their own.
                    if (element.AtomicNumber is 1 or 8 or 16 or 33)
                    {
                        continue;
                    }
                    // Aside from H, the alkali, alkaline earth, and nonmetal elements are considered
                    // disqualifying.
                    if (element.Group == 1
                        || element.Group == 2
                        || !element.Type.HasFlag(ElementType.Metal))
                    {
                        return false;
                    }
                    // Note the metal, but continue checking for disqualifying elements before returning
                    // true.
                    hasMetal = true;
                }
            }
            else
            {
                return substance.Constituents.Where(x => x.Key.Homogeneous.IsMetalOre()).Sum(x => x.Value) >= 0.5m;
            }
            return hasMetal;
        }

        /// <summary>
        /// Indicates whether the given substance is composed of either pure water, or an aqueous
        /// solution or mixture of no less than 95% water (e.g. <see
        /// cref="Substances.All.Seawater"/> would count as water).
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref name="substance"/> is entirely composed
        /// of water; otherwise <see langword="false"/>.</returns>
        public static bool IsWater(this ISubstance substance) => substance is Chemical
            ? substance.Equals(Substances.All.Water)
            : substance.GetWaterProportion() >= 0.95m;

        /// <summary>
        /// Gets a copy of the given <see cref="ISubstance"/> instance with the given <paramref
        /// name="name"/>.
        /// </summary>
        /// <param name="substance">An <see cref="ISubstance"/> instance.</param>
        /// <param name="name">A new name for the provided <see cref="ISubstance"/>
        /// instance.</param>
        /// <returns>A version of the provided <see cref="ISubstance"/> instance with the given
        /// name.</returns>
        public static T WithName<T>(this T substance, string name) where T : ISubstance
            => (T)substance.WithSubstanceName(name);

        private static (decimal carbonProportion, decimal hydrocarbonProportion, decimal waterProportion) GetHydrocarbonProportions(ISubstance substance)
        {
            if (substance is Chemical chemical)
            {
                if (chemical.IsCarbon())
                {
                    return (1, 0, 0);
                }
                else if (chemical.IsWater())
                {
                    return (0, 0, 1);
                }
                else
                {
                    var elements = chemical.Formula.Elements.ToList();
                    return elements.Count == 2
                        && elements.Any(x => x.AtomicNumber == 1)
                        && elements.Any(x => x.AtomicNumber == 6)
                        ? (0, 1, 0)
                        : (0, 0, 0);
                }
            }
            else
            {
                var carbonProportion = 0.0m;
                var hydrocarbonProportion = 0.0m;
                var waterProportion = 0.0m;
                foreach (var constituent in substance.Constituents)
                {
                    var c = constituent.Key.Homogeneous;
                    if (c is Chemical chemicalConstituent)
                    {
                        if (c.IsHydrocarbon())
                        {
                            hydrocarbonProportion += constituent.Value;
                        }
                        else if (c.IsCarbon())
                        {
                            carbonProportion += constituent.Value;
                        }
                        else if (c.IsWater())
                        {
                            waterProportion += constituent.Value;
                        }
                    }
                    else
                    {
                        var (ccp, chp, cwp) = GetHydrocarbonProportions(c);
                        carbonProportion += ccp * constituent.Value;
                        hydrocarbonProportion += chp * constituent.Value;
                        waterProportion += cwp * constituent.Value;
                    }
                }
                return (carbonProportion, hydrocarbonProportion, waterProportion);
            }
        }
    }
}
