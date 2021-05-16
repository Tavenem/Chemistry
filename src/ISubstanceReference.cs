using System;
using System.ComponentModel;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// A reference to an <see cref="ISubstance"/> instance, which can be retrieved on demand from
    /// the corresponding <see cref="Substances"/> registry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This struct is expected to be used in place of an actual <see cref="ISubstance"/> value in
    /// structures which are either persisted to memory or transmitted ovr the wire, when memory
    /// footprint is a concern.
    /// </para>
    /// <para>
    /// When serialized, this struct retains only the <see cref="ISubstance.Name"/> of the <see
    /// cref="ISubstance"/> it references. When any other property is accessed, the actual <see
    /// cref="ISubstance"/> is retrieved from the corresponding <see cref="Substances"/> registry
    /// and cached.
    /// </para>
    /// </remarks>
    [TypeConverter(typeof(SubstanceReferenceConverter))]
    [JsonInterfaceConverter(typeof(ISubstanceReferenceConverter))]
    public interface ISubstanceReference : IEquatable<ISubstanceReference>, IEquatable<ISubstance>
    {
        /// <summary>
        /// The key used to retrieve the referenced <see cref="ISubstance"/> from the <see
        /// cref="Substances"/> registry.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A string code used to prefix the <see cref="Id"/>.
        /// </summary>
        string ReferenceCode { get; }

        /// <summary>
        /// The referenced <see cref="ISubstance"/>. May retrieve <see cref="Chemical.None"/> if the
        /// key is <see langword="null"/> or not found in the <see cref="Substances"/> registry.
        /// </summary>
        ISubstance Substance { get; }
    }
}