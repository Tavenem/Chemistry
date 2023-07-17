using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry;

/// <summary>
/// The JSON contract resolver to be used by <c>Tavenem.Chemistry</c>.
/// </summary>
public class ChemistryTypeResolver : DefaultJsonTypeInfoResolver
{
    private static readonly List<JsonDerivedType> _derivedTypes = new()
    {
        new(typeof(ISubstance), $":{nameof(ISubstance)}:"),
        new(typeof(Chemical), Chemical.ChemicalIdItemTypeName),
        new(typeof(HomogeneousSubstance), HomogeneousSubstance.HomogeneousSubstanceIdItemTypeName),
        new(typeof(Mixture), Mixture.MixtureIdItemTypeName),
        new(typeof(Solution), Solution.SolutionIdItemTypeName),
    };

    /// <inheritdoc />
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type == typeof(IIdItem))
        {
            jsonTypeInfo.PolymorphismOptions ??= new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
            };
            for (var i = 0; i < _derivedTypes.Count; i++)
            {
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(_derivedTypes[i]);
            }
        }

        return jsonTypeInfo;
    }
}
