using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Tavenem.Chemistry.Elements
{
    /// <summary>
    /// <para>
    /// All known elements and their common isotopes.
    /// </para>
    /// <para>
    /// This class has mostly static members, but it also has a <see langword="static"/> <see
    /// cref="Instance"/> parameter which defines a shared instantiation of this class, and allows
    /// indexing by atomic number or <see cref="Element"/>.
    /// </para>
    /// <para>
    /// The periodic table is filled upon first use of any member (including the indexes). This
    /// means it will not cause any added memory footprint in your application unless accessed. Upon
    /// instantiation it will contain all 118 known elements, but only the most abundant,
    /// naturally-occurring isotopes of each. When an element has no naturally-occurring isotopes,
    /// only the synthetic isotope with the longest known half-life is listed. Additional <see
    /// cref="Isotope"/> instances may be added on an as-needed basis by your application using <see
    /// cref="AddIsotope(Isotope)"/>.
    /// </para>
    /// </summary>
    public sealed class PeriodicTable
    {
        /// <summary>
        /// The number of elements.
        /// </summary>
        public const byte NumberOfElements = 118;

        private static readonly Isotope[] _CommonIsotopesByAtomicNumber = new Isotope[NumberOfElements + 1];
        private static readonly Element[] _ElementsByAtomicNumber = new Element[NumberOfElements + 1];
        private static readonly Dictionary<string, Element> _ElementsBySymbol = new();
        private static readonly Dictionary<ushort, Isotope>[] _IsotopesByAtomicNumber = new Dictionary<ushort, Isotope>[NumberOfElements + 1];
        private static readonly SemaphoreSlim _Lock = new(1);

        private static bool _Initialized;

        /// <summary>
        /// The default instance of the <see cref="PeriodicTable"/>. Allows indexing.
        /// </summary>
        public static PeriodicTable Instance { get; } = new PeriodicTable();

        /// <summary>
        /// Access the element with the given atomic number.
        /// </summary>
        /// <param name="i">
        /// <para>
        /// An atomic number.
        /// </para>
        /// <para>
        /// Negative values, and those above <see cref="NumberOfElements"/> will cause an <see
        /// cref="IndexOutOfRangeException"/>.
        /// </para>
        /// <para>
        /// Zero will not cause an exception, but the result will be an empty <see cref="Element"/>.
        /// </para>
        /// </param>
        /// <returns>
        /// The element with the given atomic number.
        /// </returns>
#pragma warning disable CA1822 // Mark members as static: Not applicable to "this" indexer
        public Element this[int i]
        {
            get
            {
                if (!_Initialized)
                {
                    _Lock.Wait();
                    if (!_Initialized)
                    {
                        FillPeriodicTable();
                        _Initialized = true;
                    }
                    _Lock.Release();
                }
                return _ElementsByAtomicNumber[i];
            }
        }

        /// <summary>
        /// Access all the isotopes for the given element.
        /// </summary>
        /// <param name="element">An element.</param>
        /// <returns>An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</returns>
        public IReadOnlyDictionary<ushort, Isotope> this[Element element]
        {
            get
            {
                if (!_Initialized)
                {
                    _Lock.Wait();
                    if (!_Initialized)
                    {
                        FillPeriodicTable();
                        _Initialized = true;
                    }
                    _Lock.Release();
                }
                return GetIsotopes(element.AtomicNumber);
            }
        }
#pragma warning restore CA1822 // Mark members as static

        private PeriodicTable() { }

        /// <summary>
        /// Adds the given isotope to this instance.
        /// </summary>
        /// <param name="isotope">An isotope to add.</param>
        /// <remarks>
        /// If an isotope with the same atomic and mass numbers already exists, it will be replaced
        /// by the supplied instance.
        /// </remarks>
        public static void AddIsotope(Isotope isotope)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            AddIsotopeInternal(isotope);
        }

        /// <summary>
        /// Gets the most abundant isotope with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the isotope to retrieve.</param>
        /// <returns>The <see cref="Isotope"/> with the given atomic number which has the highest
        /// relative abundance (on Earth).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="atomicNumber"/> is zero,
        /// or greater than <see cref="NumberOfElements"/>.</exception>
        public static Isotope GetCommonIsotope(ushort atomicNumber)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            if (atomicNumber == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(atomicNumber));
            }
            return _CommonIsotopesByAtomicNumber[atomicNumber];
        }

        /// <summary>
        /// Gets the most abundant isotope of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element whose isotope is to be retrieved.</param>
        /// <returns>The <see cref="Isotope"/> of the given element which has the highest relative
        /// abundance (on Earth).</returns>
        public static Isotope GetCommonIsotope(Element element)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            return _CommonIsotopesByAtomicNumber[element.AtomicNumber];
        }

        /// <summary>
        /// Gets the element with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the element to retrieve.</param>
        /// <returns>The <see cref="Element"/> with the given atomic number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="atomicNumber"/> is zero,
        /// or greater than <see cref="NumberOfElements"/>.</exception>
        public static Element GetElement(ushort atomicNumber)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            if (atomicNumber == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(atomicNumber));
            }
            return _ElementsByAtomicNumber[atomicNumber];
        }

        /// <summary>
        /// Gets the element with the given symbol.
        /// </summary>
        /// <param name="symbol">
        /// <para>
        /// The symbol of the element to retrieve.
        /// </para>
        /// <para>
        /// Case sensitive. All element symbols begin with an uppercase character, and may be
        /// followed by a lowercase character.
        /// </para>
        /// </param>
        /// <returns>The <see cref="Element"/> with the given symbol.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="symbol"/> does not correspond
        /// with any known element.</exception>
        public static Element GetElement(string symbol)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            return _ElementsBySymbol[symbol];
        }

        /// <summary>
        /// Gets the <see cref="Isotope"/> with the given string representation.
        /// </summary>
        /// <param name="key">The string representation of an <see cref="Isotope"/>.</param>
        /// <returns>The <see cref="Isotope"/> with the given string representation.</returns>
        public static Isotope GetIsotope(string key) => IsotopeConverter.ConvertFromString(key);

        /// <summary>
        /// Gets the <see cref="Isotope"/> instances with the given string representations.
        /// </summary>
        /// <param name="keys">The string representations of <see cref="Isotope"/>
        /// instances.</param>
        /// <returns>The <see cref="Isotope"/> instances with the given string
        /// representations.</returns>
        /// <remarks>
        /// Any keys which are <see langword="null"/> or do not correspond to a valid isotope are
        /// omitted from the enumeration (they do not produce a <see langword="null"/> or default
        /// value.
        /// </remarks>
        public static IEnumerable<Isotope> GetIsotopes(IEnumerable<string>? keys)
        {
            if (keys is null)
            {
                yield break;
            }
            foreach (var key in keys)
            {
                if (IsotopeConverter.TryConvertFromString(key, out var isotope))
                {
                    yield return isotope;
                }
            }
        }

        /// <summary>
        /// Access all the isotopes with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the isotopes to retrieve.</param>
        /// <returns>An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="atomicNumber"/> is zero,
        /// or greater than <see cref="NumberOfElements"/>.</exception>
        public static IReadOnlyDictionary<ushort, Isotope> GetIsotopes(ushort atomicNumber)
        {
            if (atomicNumber == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(atomicNumber));
            }

            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            return new ReadOnlyDictionary<ushort, Isotope>(_IsotopesByAtomicNumber[atomicNumber]);
        }

        /// <summary>
        /// Access all the isotopes with the given symbol.
        /// </summary>
        /// <param name="symbol">
        /// <para>
        /// The symbol of the element to retrieve.
        /// </para>
        /// <para>
        /// Case sensitive. All element symbols begin with an uppercase character, and may be
        /// followed by a lowercase character.
        /// </para>
        /// </param>
        /// <returns>An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="symbol"/> does not correspond
        /// with any known element.</exception>
        public static IReadOnlyDictionary<ushort, Isotope> GetIsotopes(string symbol)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            return GetIsotopes(_ElementsBySymbol[symbol].AtomicNumber);
        }

        /// <summary>
        /// Access all the isotopes of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element whose isotopes are to be retrieved.</param>
        /// <returns>An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</returns>
        /// <exception cref="KeyNotFoundException">No such element exists in this <see
        /// cref="PeriodicTable"/>.</exception>
        public static IReadOnlyDictionary<ushort, Isotope> GetIsotopes(Element element)
             => GetIsotopes(element.AtomicNumber);

        /// <summary>
        /// Attempts to retrieve the most abundant isotope with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the isotope to retrieve.</param>
        /// <param name="isotope">If successful, the element found.</param>
        /// <returns><see langword="true"/> if an <see cref="Isotope"/> with the given atomic number
        /// was found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetCommonIsotope(ushort atomicNumber, out Isotope isotope)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            if (atomicNumber is 0 or > NumberOfElements)
            {
                isotope = _CommonIsotopesByAtomicNumber[1];
                return false;
            }
            isotope = _CommonIsotopesByAtomicNumber[atomicNumber];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the most abundant isotope of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element whose isotope is to be retrieved.</param>
        /// <param name="isotope">If successful, the element found.</param>
        /// <returns><see langword="true"/> if an <see cref="Isotope"/> of the given element was
        /// found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetCommonIsotope(Element element, out Isotope isotope)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            isotope = _CommonIsotopesByAtomicNumber[element.AtomicNumber];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve an element with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the element to retrieve.</param>
        /// <param name="element">If successful, the element found.</param>
        /// <returns><see langword="true"/> if an <see cref="Element"/> with the given symbol was
        /// found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetElement(ushort atomicNumber, out Element element)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            if (atomicNumber is 0 or > NumberOfElements)
            {
                element = _ElementsByAtomicNumber[1];
                return false;
            }
            element = _ElementsByAtomicNumber[atomicNumber];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve an element with the given symbol.
        /// </summary>
        /// <param name="symbol">
        /// <para>
        /// The symbol of the element to retrieve.
        /// </para>
        /// <para>
        /// Case sensitive. All element symbols begin with an uppercase character, and may be
        /// followed by a lowercase character.
        /// </para>
        /// </param>
        /// <param name="element">If successful, the element found.</param>
        /// <returns><see langword="true"/> if an <see cref="Element"/> with the given symbol was
        /// found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetElement(string symbol, out Element element)
        {
            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            return _ElementsBySymbol.TryGetValue(symbol, out element);
        }

        /// <summary>
        /// Attempts to get the <see cref="Isotope"/> with the given string representation.
        /// </summary>
        /// <param name="key">The string representation of an <see cref="Isotope"/>.</param>
        /// <param name="isotope">If successful, the <see cref="Isotope"/> with the given string
        /// representation.</param>
        /// <returns><see langword="true"/> if the <see cref="Isotope"/> was successfully retrieved;
        /// otherwise <see langword="false"/>.</returns>
        public static bool TryGetIsotope(string key, out Isotope isotope)
            => IsotopeConverter.TryConvertFromString(key, out isotope);

        /// <summary>
        /// Attempts to access all the isotopes with the given atomic number.
        /// </summary>
        /// <param name="atomicNumber">The atomic number of the isotopes to retrieve.</param>
        /// <param name="isotopes">If successful, will be set to an <see
        /// cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</param>
        /// <returns><see langword="true"/> if the isotopes were successfully retrieved; otherwise
        /// <see langword="false"/>.</returns>
        public static bool TryGetIsotopes(ushort atomicNumber, out IReadOnlyDictionary<ushort, Isotope> isotopes)
        {
            if (atomicNumber is 0 or > NumberOfElements)
            {
                isotopes = new ReadOnlyDictionary<ushort, Isotope>(new Dictionary<ushort, Isotope>());
                return false;
            }

            if (!_Initialized)
            {
                _Lock.Wait();
                if (!_Initialized)
                {
                    FillPeriodicTable();
                    _Initialized = true;
                }
                _Lock.Release();
            }

            isotopes = new ReadOnlyDictionary<ushort, Isotope>(_IsotopesByAtomicNumber[atomicNumber]);
            return true;
        }

        /// <summary>
        /// Attempts to access all the isotopes with the given symbol.
        /// </summary>
        /// <param name="symbol">
        /// <para>
        /// The symbol of the element to retrieve.
        /// </para>
        /// <para>
        /// Case sensitive. All element symbols begin with an uppercase character, and may be
        /// followed by a lowercase character.
        /// </para>
        /// </param>
        /// <param name="isotopes">If successful, will be set to an <see
        /// cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</param>
        /// <returns><see langword="true"/> if the isotopes were successfully retrieved; otherwise
        /// <see langword="false"/>.</returns>
        public static bool TryGetIsotopes(string symbol, out IReadOnlyDictionary<ushort, Isotope> isotopes)
        {
            if (!TryGetElement(symbol, out var element))
            {
                isotopes = new ReadOnlyDictionary<ushort, Isotope>(new Dictionary<ushort, Isotope>());
                return false;
            }
            return TryGetIsotopes(element, out isotopes);
        }

        /// <summary>
        /// Attempts to access all the isotopes of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element whose isotopes are to be retrieved.</param>
        /// <param name="isotopes">If successful, will be set to an <see
        /// cref="IReadOnlyDictionary{TKey, TValue}"/> of <see cref="Isotope"/>
        /// instances, indexed by mass number.</param>
        /// <returns><see langword="true"/> if the isotopes were successfully retrieved; otherwise
        /// <see langword="false"/>.</returns>
        public static bool TryGetIsotopes(Element element, out IReadOnlyDictionary<ushort, Isotope> isotopes)
            => TryGetIsotopes(element.AtomicNumber, out isotopes);

        private static void AddIsotopeInternal(Isotope isotope)
        {
            if (_IsotopesByAtomicNumber[isotope.AtomicNumber] is null)
            {
                _IsotopesByAtomicNumber[isotope.AtomicNumber] = new Dictionary<ushort, Isotope>();
            }
            _IsotopesByAtomicNumber[isotope.AtomicNumber][isotope.MassNumber] = isotope;
        }

        private static void FillPeriodicTable()
        {
            AddElement(1, 1.008, 's', new ElectronConfiguration(new Orbital(1, 's', 1)), 1, new bool[] { false, false }, new ushort[] { 1, 2 }, "Hydrogen", 1, new[] { 0.9998, 0.0002 }, "H", ElementType.ReactiveNonmetal);
            var ec = new ElectronConfiguration(new Orbital(1, 's', 2));
            AddElement(2, 4.002602, 's', ec, 18, new bool[] { false, false }, new ushort[] { 4, 3 }, "Helium", 1, new[] { 0.999998, 0.000002 }, "He", ElementType.NobleGas);
            AddElement(3, 6.94, 's', new ElectronConfiguration(ec, new Orbital(2, 's', 1)), 1, new bool[] { false, false }, new ushort[] { 7, 6 }, "Lithium", 2, new[] { 0.9625, 0.0375 }, "Li", ElementType.Alkali);
            ec = new ElectronConfiguration(ec, new Orbital(2, 's', 2));
            AddElement(4, 9.0121831, 's', ec, 2, new bool[] { false }, new ushort[] { 9 }, "Beryllium", 2, new[] { 1.0 }, "Be", ElementType.AlkalineEarth);
            AddElement(5, 10.81, 'p', new ElectronConfiguration(ec, new Orbital(2, 'p', 1)), 13, new bool[] { false, false, }, new ushort[] { 11, 10 }, "Boron", 2, new[] { 0.8, 0.2 }, "B", ElementType.ReactiveNonmetal | ElementType.Metalloid);
            AddElement(6, 12.011, 'p', new ElectronConfiguration(ec, new Orbital(2, 'p', 2)), 14, new bool[] { false, false }, new ushort[] { 12, 13 }, "Carbon", 2, new[] { 0.989, 0.011 }, "C", ElementType.ReactiveNonmetal);
            AddElement(7, 14.007, 'p', new ElectronConfiguration(ec, new Orbital(2, 'p', 3)), 15, new bool[] { false, false }, new ushort[] { 14, 15 }, "Nitrogen", 2, new[] { 0.996, 0.004 }, "N", ElementType.ReactiveNonmetal | ElementType.Pnictogen);
            AddElement(8, 15.999, 'p', new ElectronConfiguration(ec, new Orbital(2, 'p', 4)), 16, new bool[] { false, false, false }, new ushort[] { 16, 18, 17 }, "Oxygen", 2, new[] { 0.9976, 0.002, 0.0004 }, "O", ElementType.ReactiveNonmetal | ElementType.Chalcogen);
            AddElement(9, 18.99840316, 'p', new ElectronConfiguration(ec, new Orbital(2, 'p', 5)), 17, new bool[] { false }, new ushort[] { 19 }, "Fluorine", 2, new[] { 1.0 }, "F", ElementType.ReactiveNonmetal | ElementType.Halogen);
            ec = new ElectronConfiguration(ec, new Orbital(2, 'p', 6));
            AddElement(10, 20.1797, 'p', ec, 18, new bool[] { false, false, false }, new ushort[] { 20, 22, 21 }, "Neon", 2, new[] { 0.9048, 0.0925, 0.0027 }, "Ne", ElementType.NobleGas);
            AddElement(11, 22.98976928, 's', new ElectronConfiguration(ec, new Orbital(3, 's', 1)), 1, new bool[] { false }, new ushort[] { 23 }, "Sodium", 3, new[] { 1.0 }, "Na", ElementType.Alkali);
            ec = new ElectronConfiguration(ec, new Orbital(3, 's', 2));
            AddElement(12, 24.305, 's', ec, 2, new bool[] { false, false, false }, new ushort[] { 24, 26, 25 }, "Magnesium", 3, new[] { 0.79, 0.11, 0.1 }, "Mg", ElementType.AlkalineEarth);
            AddElement(13, 26.9815385, 'p', new ElectronConfiguration(ec, new Orbital(3, 'p', 1)), 13, new bool[] { false }, new ushort[] { 27 }, "Aluminium", 3, new[] { 1.0 }, "Al", ElementType.PostTransition);
            AddElement(14, 28.085, 'p', new ElectronConfiguration(ec, new Orbital(3, 'p', 2)), 14, new bool[] { false, false, false }, new ushort[] { 28, 29, 30 }, "Silicon", 3, new[] { 0.922, 0.047, 0.031 }, "Si", ElementType.ReactiveNonmetal | ElementType.Metalloid);
            AddElement(15, 30.973762, 'p', new ElectronConfiguration(ec, new Orbital(3, 'p', 3)), 15, new bool[] { false }, new ushort[] { 31 }, "Phosphorus", 3, new[] { 1.0 }, "P", ElementType.ReactiveNonmetal | ElementType.Pnictogen);
            AddElement(16, 32.06, 'p', new ElectronConfiguration(ec, new Orbital(3, 'p', 4)), 16, new bool[] { false, false, false, false }, new ushort[] { 32, 34, 33, 36 }, "Sulfur", 3, new[] { 0.9499, 0.0425, 0.0075, 0.0001 }, "S", ElementType.ReactiveNonmetal | ElementType.Chalcogen);
            AddElement(17, 35.45, 'p', new ElectronConfiguration(ec, new Orbital(3, 'p', 5)), 17, new bool[] { false, false }, new ushort[] { 35, 37 }, "Chlorine", 3, new[] { 0.76, 0.24 }, "Cl", ElementType.ReactiveNonmetal | ElementType.Halogen);
            ec = new ElectronConfiguration(ec, new Orbital(3, 'p', 6));
            AddElement(18, 39.948, 'p', ec, 18, new bool[] { false, false, false }, new ushort[] { 40, 36, 38 }, "Argon", 3, new[] { 0.99604, 0.00334, 0.00063 }, "Ar", ElementType.NobleGas);
            var ec1 = new ElectronConfiguration(ec, new Orbital(4, 's', 1));
            AddElement(19, 39.0983, 's', ec, 1, new bool[] { false, false, true }, new ushort[] { 39, 41, 40 }, "Potassium", 4, new[] { 0.93258, 0.0673, 0.00012 }, "K", ElementType.Alkali);
            var ec2 = new ElectronConfiguration(ec, new Orbital(4, 's', 2));
            AddElement(20, 40.078, 's', ec2, 2, new bool[] { false, false, false, true, false, false }, new ushort[] { 40, 44, 42, 48, 43, 46 }, "Calcium", 4, new[] { 0.96941, 0.02086, 0.00647, 0.00187, 0.00135, 0.00004 }, "Ca", ElementType.AlkalineEarth);
            AddElement(21, 44.955908, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 1)), 3, new bool[] { false }, new ushort[] { 45 }, "Scandium", 4, new[] { 1.0 }, "Sc", ElementType.Transition | ElementType.Group3);
            AddElement(22, 47.867, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 2)), 4, new bool[] { false, false, false, false, false }, new ushort[] { 48, 46, 47, 49, 50 }, "Titanium", 4, new[] { 0.7372, 0.0825, 0.0744, 0.0541, 0.0518 }, "Ti", ElementType.Transition);
            AddElement(23, 50.9415, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 3)), 5, new bool[] { false, true }, new ushort[] { 51, 50 }, "Vanadium", 4, new[] { 0.9975, 0.0025 }, "V", ElementType.Transition);
            AddElement(24, 51.9961, 'd', new ElectronConfiguration(ec1, new Orbital(3, 'd', 5)), 6, new bool[] { false, false, false, false }, new ushort[] { 52, 53, 50, 54 }, "Chromium", 4, new[] { 0.83789, 0.09501, 0.04345, 0.02365 }, "Cr", ElementType.Transition);
            AddElement(25, 54.938044, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 5)), 7, new bool[] { false }, new ushort[] { 55 }, "Manganese", 4, new[] { 1.0 }, "Mn", ElementType.Transition);
            AddElement(26, 55.845, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 6)), 8, new bool[] { false, false, false, false }, new ushort[] { 56, 54, 57, 58 }, "Iron", 4, new[] { 0.9175, 0.0585, 0.0212, 0.0028 }, "Fe", ElementType.Transition);
            AddElement(27, 58.933194, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 7)), 9, new bool[] { false }, new ushort[] { 59 }, "Cobalt", 4, new[] { 1.0 }, "Co", ElementType.Transition);
            AddElement(28, 58.6934, 'd', new ElectronConfiguration(ec2, new Orbital(3, 'd', 8)), 10, new bool[] { false, false, false, false, false }, new ushort[] { 58, 60, 62, 61, 64 }, "Nickel", 4, new[] { 0.68077, 0.26223, 0.03635, 0.01140, 0.00926 }, "Ni", ElementType.Transition);
            AddElement(29, 63.546, 'd', new ElectronConfiguration(ec1, new Orbital(3, 'd', 10)), 11, new bool[] { false, false }, new ushort[] { 63, 65 }, "Copper", 4, new[] { 0.6915, 0.3085 }, "Cu", ElementType.Transition);
            ec = new ElectronConfiguration(ec2, new Orbital(3, 'd', 10));
            AddElement(30, 65.38, 'd', ec, 12, new bool[] { false, false, false, false, false }, new ushort[] { 64, 66, 68, 67, 70 }, "Zinc", 4, new[] { 0.492, 0.277, 0.185, 0.04, 0.006 }, "Zn", ElementType.PostTransition);
            AddElement(31, 69.723, 'p', new ElectronConfiguration(ec, new Orbital(4, 'p', 1)), 13, new bool[] { false, false }, new ushort[] { 69, 71 }, "Gallium", 4, new[] { 0.6011, 0.3989 }, "Ga", ElementType.PostTransition);
            AddElement(32, 72.63, 'p', new ElectronConfiguration(ec, new Orbital(4, 'p', 2)), 14, new bool[] { false, false, false, false, true }, new ushort[] { 74, 72, 70, 73, 76 }, "Germanium", 4, new[] { 0.367, 0.2745, 0.2052, 0.0776, 0.0775 }, "Ge", ElementType.PostTransition | ElementType.Metalloid | ElementType.Pnictogen);
            AddElement(33, 74.921595, 'p', new ElectronConfiguration(ec, new Orbital(4, 'p', 3)), 15, new bool[] { false }, new ushort[] { 75 }, "Arsenic", 4, new[] { 1.0 }, "As", ElementType.ReactiveNonmetal | ElementType.Metalloid);
            AddElement(34, 78.971, 'p', new ElectronConfiguration(ec, new Orbital(4, 'p', 4)), 16, new bool[] { false, false, false, true, false, false }, new ushort[] { 80, 78, 76, 82, 77, 74 }, "Selenium", 4, new[] { 0.498, 0.2369, 0.0923, 0.0882, 0.076, 0.0086 }, "Se", ElementType.ReactiveNonmetal | ElementType.Chalcogen);
            AddElement(35, 79.904, 'p', new ElectronConfiguration(ec, new Orbital(4, 'p', 5)), 17, new bool[] { false, false }, new ushort[] { 79, 81 }, "Bromine", 4, new[] { 0.51, 0.49 }, "Br", ElementType.ReactiveNonmetal | ElementType.Halogen);
            ec = new ElectronConfiguration(ec, new Orbital(4, 'p', 6));
            AddElement(36, 83.798, 'p', ec, 18, new bool[] { false, false, false, false, false, true }, new ushort[] { 84, 86, 82, 83, 80, 78 }, "Krypton", 4, new[] { 0.5699, 0.1728, 0.1159, 0.115, 0.0229, 0.0036 }, "Kr", ElementType.NobleGas);
            ec1 = new ElectronConfiguration(ec, new Orbital(5, 's', 1));
            AddElement(37, 85.4678, 's', ec1, 1, new bool[] { false, true }, new ushort[] { 85, 87 }, "Rubidium", 5, new[] { 0.7217, 0.2783 }, "Rb", ElementType.Alkali);
            ec2 = new ElectronConfiguration(ec, new Orbital(5, 's', 2));
            AddElement(38, 87.62, 's', ec2, 2, new bool[] { false, false, false, false }, new ushort[] { 88, 86, 87, 84 }, "Strontium", 5, new[] { 0.8258, 0.0986, 0.07, 0.0056 }, "Sr", ElementType.AlkalineEarth);
            AddElement(39, 88.90584, 'd', new ElectronConfiguration(ec2, new Orbital(4, 'd', 1)), 3, new bool[] { false }, new ushort[] { 89 }, "Yttrium", 5, new[] { 1.0 }, "Y", ElementType.Transition | ElementType.Group3);
            AddElement(40, 91.224, 'd', new ElectronConfiguration(ec2, new Orbital(4, 'd', 2)), 4, new bool[] { false, false, false, false, true }, new ushort[] { 90, 94, 92, 91, 96 }, "Zirconium", 5, new[] { 0.5145, 0.1738, 0.1715, 0.1122, 0.028 }, "Zr", ElementType.Transition);
            AddElement(41, 92.90637, 'd', new ElectronConfiguration(ec1, new Orbital(4, 'd', 4)), 5, new bool[] { false }, new ushort[] { 93 }, "Niobium", 5, new[] { 1.0 }, "Nb", ElementType.Transition);
            AddElement(42, 95.95, 'd', new ElectronConfiguration(ec1, new Orbital(4, 'd', 5)), 6, new bool[] { false, false, false, false, true, false, false }, new ushort[] { 98, 96, 95, 92, 100, 97, 94 }, "Molybdenum", 5, new[] { 0.2429, 0.1667, 0.1587, 0.1465, 0.0974, 0.0958, 0.0919 }, "Mo", ElementType.Transition);
            AddElement(43, 98, 'd', new ElectronConfiguration(ec2, new Orbital(4, 'd', 5)), 7, new bool[] { true }, new ushort[] { 99 }, "Technetium", 5, new[] { 1.0 }, "Tc", ElementType.Transition);
            AddElement(44, 101.07, 'd', new ElectronConfiguration(ec1, new Orbital(4, 'd', 7)), 8, new bool[] { false, false, false, false, false, false, false }, new ushort[] { 102, 104, 101, 99, 100, 96, 98 }, "Ruthenium", 5, new[] { 0.3155, 0.1862, 0.1706, 0.1276, 0.126, 0.0554, 0.0187 }, "Ru", ElementType.Transition);
            AddElement(45, 102.9055, 'd', new ElectronConfiguration(ec1, new Orbital(4, 'd', 8)), 9, new bool[] { false }, new ushort[] { 103 }, "Rhodium", 5, new[] { 1.0 }, "Rh", ElementType.Transition);
            ec = new ElectronConfiguration(ec, new Orbital(4, 'd', 10));
            AddElement(46, 106.42, 'd', ec, 10, new bool[] { false, false, false, false, false, false }, new ushort[] { 106, 108, 105, 110, 104, 102 }, "Palladium", 5, new[] { 0.2733, 0.2646, 0.2233, 0.1172, 0.1114, 0.0102 }, "Pd", ElementType.Transition);
            AddElement(47, 107.8682, 'd', new ElectronConfiguration(ec, new Orbital(5, 's', 1)), 11, new bool[] { false, false }, new ushort[] { 107, 109 }, "Silver", 5, new[] { 0.51839, 0.48161 }, "Ag", ElementType.Transition);
            ec = new ElectronConfiguration(ec, new Orbital(5, 's', 2));
            AddElement(48, 112.414, 'd', ec, 12, new bool[] { false, false, false, false, true, true, false, false }, new ushort[] { 114, 112, 111, 110, 113, 116, 106, 108 }, "Cadmium", 5, new[] { 0.2875, 0.2411, 0.1280, 0.1247, 0.1223, 0.0751, 0.0125, 0.0089 }, "Cd", ElementType.PostTransition);
            AddElement(49, 114.818, 'p', new ElectronConfiguration(ec, new Orbital(5, 'p', 1)), 13, new bool[] { true, false }, new ushort[] { 115, 113 }, "Indium", 5, new[] { 0.9572, 0.0428 }, "In", ElementType.PostTransition);
            AddElement(50, 118.71, 'p', new ElectronConfiguration(ec, new Orbital(5, 'p', 2)), 14, new bool[] { false, false, false, false, false, false, false, false, false, false }, new ushort[] { 120, 118, 116, 119, 117, 124, 122, 112, 114, 115 }, "Tin", 5, new[] { 0.3258, 0.2422, 0.1454, 0.0859, 0.0768, 0.0579, 0.0463, 0.0097, 0.0066, 0.0034 }, "Sn", ElementType.PostTransition);
            AddElement(51, 121.76, 'p', new ElectronConfiguration(ec, new Orbital(5, 'p', 3)), 15, new bool[] { false, false }, new ushort[] { 121, 122 }, "Antimony", 5, new[] { 0.5721, 0.4279 }, "Sb", ElementType.PostTransition | ElementType.Metalloid | ElementType.Pnictogen);
            AddElement(52, 127.6, 'p', new ElectronConfiguration(ec, new Orbital(5, 'p', 4)), 16, new bool[] { true, true, false, false, false, false, false, false }, new ushort[] { 130, 128, 126, 125, 124, 122, 123, 120 }, "Tellurium", 5, new[] { 0.3408, 0.3174, 0.1884, 0.0707, 0.0474, 0.0255, 0.0089, 0.0009 }, "Te", ElementType.ReactiveNonmetal | ElementType.Metalloid | ElementType.Chalcogen);
            AddElement(53, 126.90447, 'p', new ElectronConfiguration(ec, new Orbital(5, 'p', 5)), 17, new bool[] { false }, new ushort[] { 127 }, "Iodine", 5, new[] { 1.0 }, "I", ElementType.ReactiveNonmetal | ElementType.Halogen);
            ec = new ElectronConfiguration(ec, new Orbital(5, 'p', 6));
            AddElement(54, 131.293, 'p', ec, 18, new bool[] { false, false, false, false, true, false, false, false, false }, new ushort[] { 132, 129, 131, 134, 136, 130, 128, 124, 126 }, "Xenon", 5, new[] { 0.26909, 0.26401, 0.21232, 0.10436, 0.08857, 0.04071, 0.0191, 0.00095, 0.00089 }, "Xe", ElementType.NobleGas);
            ec1 = new ElectronConfiguration(ec, new Orbital(6, 's', 1));
            AddElement(55, 132.905452, 's', ec1, 1, new bool[] { false }, new ushort[] { 133 }, "Caesium", 6, new[] { 1.0 }, "Cs", ElementType.Alkali);
            ec2 = new ElectronConfiguration(ec, new Orbital(6, 's', 2));
            AddElement(56, 137.327, 's', ec2, 2, new bool[] { false, false, false, false, false, true, false }, new ushort[] { 138, 137, 136, 135, 134, 130, 132 }, "Barium", 6, new[] { 0.717, 0.1123, 0.0785, 0.0659, 0.0242, 0.0011, 0.001 }, "Ba", ElementType.AlkalineEarth);
            AddElement(57, 138.90547, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 1)), 3, new bool[] { false, true }, new ushort[] { 139, 138 }, "Lanthanum", 6, new[] { 0.99911, 0.00089 }, "La", ElementType.Lanthanide);
            AddElement(58, 140.116, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 1), new Orbital(5, 'd', 1)), null, new bool[] { false, false, false, false }, new ushort[] { 140, 142, 138, 136 }, "Cerium", 6, new[] { 0.88449, 0.11114, 0.00251, 0.00186 }, "Ce", ElementType.Lanthanide);
            AddElement(59, 140.90766, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 3)), null, new bool[] { false }, new ushort[] { 141 }, "Praseodymium", 6, new[] { 1.0 }, "Pr", ElementType.Lanthanide);
            AddElement(60, 144.242, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 4)), null, new bool[] { false, true, false, false, false, false, true }, new ushort[] { 142, 144, 146, 143, 145, 148, 150 }, "Neodymium", 6, new[] { 0.272, 0.238, 0.172, 0.122, 0.083, 0.058, 0.056 }, "Nd", ElementType.Lanthanide);
            AddElement(61, 145, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 5)), null, new bool[] { true }, new ushort[] { 145 }, "Promethium", 6, new[] { 1.0 }, "Pm", ElementType.Lanthanide);
            AddElement(62, 150.36, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 6)), null, new bool[] { false, false, true, false, true, false, false }, new ushort[] { 152, 154, 147, 149, 148, 150, 144 }, "Samarium", 6, new[] { 0.2674, 0.2274, 0.15, 0.1382, 0.1125, 0.0737, 0.0308 }, "Sm", ElementType.Lanthanide);
            AddElement(63, 151.964, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 7)), null, new bool[] { false, true }, new ushort[] { 153, 151 }, "Europium", 6, new[] { 0.522, 0.478 }, "Eu", ElementType.Lanthanide);
            AddElement(64, 157.25, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 7), new Orbital(5, 'd', 1)), null, new bool[] { false, false, false, false, false, false, true }, new ushort[] { 158, 160, 156, 157, 155, 154, 152 }, "Gadolinium", 6, new[] { 0.2484, 0.2186, 0.2047, 0.1565, 0.148, 0.0218, 0.002 }, "Gd", ElementType.Lanthanide);
            AddElement(65, 158.92535, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 9)), null, new bool[] { false }, new ushort[] { 159 }, "Terbium", 6, new[] { 1.0 }, "Tb", ElementType.Lanthanide);
            AddElement(66, 162.5, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 10)), null, new bool[] { false, false, false, false, false, false, false }, new ushort[] { 164, 162, 163, 161, 160, 158, 156 }, "Dysprosium", 6, new[] { 0.2826, 0.25475, 0.24896, 0.18889, 0.02329, 0.00095, 0.00056 }, "Dy", ElementType.Lanthanide);
            AddElement(67, 164.93033, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 11)), null, new bool[] { false }, new ushort[] { 165 }, "Holmium", 6, new[] { 1.0 }, "Ho", ElementType.Lanthanide);
            AddElement(68, 167.259, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 12)), null, new bool[] { false, false, false, false, false, false }, new ushort[] { 166, 168, 167, 170, 164, 162 }, "Erbium", 6, new[] { 0.33503, 0.26978, 0.22869, 0.1491, 0.01601, 0.00139 }, "Er", ElementType.Lanthanide);
            AddElement(69, 168.93422, 'f', new ElectronConfiguration(ec2, new Orbital(4, 'f', 13)), null, new bool[] { false }, new ushort[] { 169 }, "Thulium", 6, new[] { 1.0 }, "Tm", ElementType.Lanthanide);
            ec = new ElectronConfiguration(ec, new Orbital(4, 'f', 14));
            ec2 = new ElectronConfiguration(ec, new Orbital(6, 's', 2));
            AddElement(70, 173.045, 'f', ec2, null, new bool[] { false, false, false, false, false, false, false }, new ushort[] { 174, 172, 173, 171, 176, 170, 168 }, "Ytterbium", 6, new[] { 0.31896, 0.21754, 0.16098, 0.14216, 0.12887, 0.03023, 0.00126 }, "Yb", ElementType.Lanthanide);
            AddElement(71, 174.9668, 'f', new ElectronConfiguration(ec2, new Orbital(5, 'd', 1)), null, new bool[] { false, true }, new ushort[] { 175, 176 }, "Lutetium", 6, new[] { 0.97401, 0.02599 }, "Lu", ElementType.Lanthanide);
            AddElement(72, 178.49, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 2)), 4, new bool[] { false, false, false, false, false, true }, new ushort[] { 180, 178, 177, 179, 176, 174 }, "Hafnium", 6, new[] { 0.3508, 0.2728, 0.186, 0.1362, 0.0526, 0.0016 }, "Hf", ElementType.Transition);
            AddElement(73, 180.94788, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 3)), 5, new bool[] { false, false }, new ushort[] { 181, 180 }, "Tantalum", 6, new[] { 0.99988, 0.00012 }, "Ta", ElementType.Transition);
            AddElement(74, 183.84, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 4)), 6, new bool[] { false, false, false, false, false }, new ushort[] { 184, 186, 182, 183, 180 }, "Tungsten", 6, new[] { 0.3064, 0.2843, 0.265, 0.1431, 0.0012 }, "W", ElementType.Transition);
            AddElement(75, 186.207, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 5)), 7, new bool[] { true, false }, new ushort[] { 187, 185 }, "Rhenium", 6, new[] { 0.626, 0.374 }, "Re", ElementType.Transition);
            AddElement(76, 190.23, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 6)), 8, new bool[] { false, false, false, false, false, true, false }, new ushort[] { 192, 190, 189, 188, 187, 186, 184 }, "Osmium", 6, new[] { 0.4078, 0.2626, 0.1615, 0.1324, 0.0196, 0.0159, 0.0002 }, "Os", ElementType.Transition);
            AddElement(77, 192.217, 'd', new ElectronConfiguration(ec2, new Orbital(5, 'd', 7)), 9, new bool[] { false, false }, new ushort[] { 193, 191 }, "Iridium", 6, new[] { 0.627, 0.373 }, "Ir", ElementType.Transition);
            AddElement(78, 195.084, 'd', new ElectronConfiguration(ec, new Orbital(5, 'd', 9), new Orbital(6, 's', 1)), 10, new bool[] { false, false, false, false, false, false }, new ushort[] { 195, 194, 196, 198, 192, 190 }, "Platinum", 6, new[] { 0.33775, 0.32864, 0.25211, 0.07356, 0.00782, 0.00012 }, "Pt", ElementType.Transition);
            AddElement(79, 196.966569, 'd', new ElectronConfiguration(ec, new Orbital(5, 'd', 10), new Orbital(6, 's', 1)), 11, new bool[] { false }, new ushort[] { 197 }, "Gold", 6, new[] { 1.0 }, "Au", ElementType.Transition);
            ec = new ElectronConfiguration(ec2, new Orbital(5, 'd', 10));
            AddElement(80, 200.592, 'd', ec, 12, new bool[] { false, false, false, false, false, false, false }, new ushort[] { 202, 200, 199, 201, 198, 204, 196 }, "Mercury", 6, new[] { 0.2974, 0.2314, 0.1694, 0.1317, 0.1004, 0.0682, 0.0015 }, "Hg", ElementType.PostTransition);
            AddElement(81, 204.38, 'p', new ElectronConfiguration(ec, new Orbital(6, 'p', 1)), 13, new bool[] { false, false }, new ushort[] { 205, 203 }, "Thallium", 6, new[] { 0.705, 0.295 }, "Tl", ElementType.PostTransition);
            AddElement(82, 207.2, 'p', new ElectronConfiguration(ec, new Orbital(6, 'p', 2)), 14, new bool[] { false, false, false, false }, new ushort[] { 208, 206, 207, 204 }, "Lead", 6, new[] { 0.524, 0.241, 0.221, 0.014 }, "Pb", ElementType.PostTransition);
            AddElement(83, 208.9804, 'p', new ElectronConfiguration(ec, new Orbital(6, 'p', 3)), 15, new bool[] { true }, new ushort[] { 209 }, "Bismuth", 6, new[] { 1.0 }, "Bi", ElementType.PostTransition | ElementType.Pnictogen);
            AddElement(84, 209, 'p', new ElectronConfiguration(ec, new Orbital(6, 'p', 4)), 16, new bool[] { true }, new ushort[] { 209 }, "Polonium", 6, new[] { 1.0 }, "Po", ElementType.PostTransition | ElementType.Metalloid | ElementType.Chalcogen);
            AddElement(85, 210, 'p', new ElectronConfiguration(ec, new Orbital(6, 'p', 5)), 17, new bool[] { true }, new ushort[] { 210 }, "Astatine", 6, new[] { 1.0 }, "At", ElementType.ReactiveNonmetal | ElementType.Metalloid | ElementType.Halogen);
            ec = new ElectronConfiguration(ec, new Orbital(6, 'p', 6));
            AddElement(86, 222, 'p', ec, 18, new bool[] { true }, new ushort[] { 222 }, "Radon", 6, new[] { 1.0 }, "Rn", ElementType.NobleGas);
            AddElement(87, 223, 's', new ElectronConfiguration(ec, new Orbital(7, 's', 1)), 1, new bool[] { true }, new ushort[] { 223 }, "Francium", 7, new[] { 1.0 }, "Fr", ElementType.Alkali);
            ec = new ElectronConfiguration(ec, new Orbital(7, 's', 2));
            AddElement(88, 226, 's', ec, 2, new bool[] { true }, new ushort[] { 226 }, "Radium", 7, new[] { 1.0 }, "Ra", ElementType.AlkalineEarth);
            ec1 = new ElectronConfiguration(ec, new Orbital(6, 'd', 1));
            AddElement(89, 227, 'd', ec1, 3, new bool[] { true }, new ushort[] { 227 }, "Actinium", 7, new[] { 1.0 }, "Ac", ElementType.Actinide);
            AddElement(90, 232.0377, 'f', new ElectronConfiguration(ec, new Orbital(6, 'd', 2)), null, new bool[] { true, true }, new ushort[] { 232, 230 }, "Thorium", 7, new[] { 0.9998, 0.0002 }, "Th", ElementType.Actinide);
            AddElement(91, 231.03588, 'f', new ElectronConfiguration(ec1, new Orbital(5, 'f', 2)), null, new bool[] { true }, new ushort[] { 231 }, "Protactinium", 7, new[] { 1.0 }, "Pa", ElementType.Actinide);
            AddElement(92, 238.02891, 'f', new ElectronConfiguration(ec1, new Orbital(5, 'f', 3)), null, new bool[] { true, true, true }, new ushort[] { 238, 235, 234 }, "Uranium", 7, new[] { 0.99274, 0.00720, 0.00005 }, "U", ElementType.Actinide);
            AddElement(93, 237, 'f', new ElectronConfiguration(ec1, new Orbital(5, 'f', 4)), null, new bool[] { true }, new ushort[] { 237 }, "Neptunium", 7, new[] { 1.0 }, "Np", ElementType.Actinide);
            AddElement(94, 244, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 6)), null, new bool[] { true }, new ushort[] { 244 }, "Plutonium", 7, new[] { 1.0 }, "Pu", ElementType.Actinide);
            AddElement(95, 243, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 7)), null, new bool[] { true }, new ushort[] { 243 }, "Americium", 7, new[] { 1.0 }, "Am", ElementType.Actinide);
            AddElement(96, 247, 'f', new ElectronConfiguration(ec1, new Orbital(5, 'f', 7)), null, new bool[] { true }, new ushort[] { 247 }, "Curium", 7, new[] { 1.0 }, "Cm", ElementType.Actinide);
            AddElement(97, 247, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 9)), null, new bool[] { true }, new ushort[] { 247 }, "Berkelium", 7, new[] { 1.0 }, "Bk", ElementType.Actinide);
            AddElement(98, 251, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 10)), null, new bool[] { true }, new ushort[] { 251 }, "Californium", 7, new[] { 1.0 }, "Cf", ElementType.Actinide);
            AddElement(99, 252, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 11)), null, new bool[] { true }, new ushort[] { 252 }, "Einsteinium", 7, new[] { 1.0 }, "Es", ElementType.Actinide);
            AddElement(100, 257, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 12)), null, new bool[] { true }, new ushort[] { 257 }, "Fermium", 7, new[] { 1.0 }, "Fm", ElementType.Actinide);
            AddElement(101, 258, 'f', new ElectronConfiguration(ec, new Orbital(5, 'f', 13)), null, new bool[] { true }, new ushort[] { 258 }, "Mendelevium", 7, new[] { 1.0 }, "Md", ElementType.Actinide);
            ec = new ElectronConfiguration(ec, new Orbital(5, 'f', 14));
            AddElement(102, 259, 'f', ec, null, new bool[] { true }, new ushort[] { 259 }, "Nobelium", 7, new[] { 1.0 }, "No", ElementType.Actinide);
            AddElement(103, 266, 'f', new ElectronConfiguration(ec, new Orbital(7, 'p', 1)), null, new bool[] { true }, new ushort[] { 266 }, "Lawrencium", 7, new[] { 1.0 }, "Lr", ElementType.Actinide);
            AddElement(104, 267, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 2)), 4, new bool[] { true }, new ushort[] { 267 }, "Rutherfordium", 7, new[] { 1.0 }, "Rf", ElementType.Transition);
            AddElement(105, 268, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 3)), 5, new bool[] { true }, new ushort[] { 268 }, "Dubnium", 7, new[] { 1.0 }, "Db", ElementType.Transition);
            AddElement(106, 269, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 4)), 6, new bool[] { true }, new ushort[] { 269 }, "Seaborgium", 7, new[] { 1.0 }, "Sg", ElementType.Transition);
            AddElement(107, 270, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 5)), 7, new bool[] { true }, new ushort[] { 270 }, "Bohrium", 7, new[] { 1.0 }, "Bh", ElementType.Transition);
            AddElement(108, 277, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 6)), 8, new bool[] { true }, new ushort[] { 277 }, "Hassium", 7, new[] { 1.0 }, "Hs", ElementType.Transition);
            AddElement(109, 278, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 7)), 9, new bool[] { true }, new ushort[] { 278 }, "Meitnerium", 7, new[] { 1.0 }, "Mt", ElementType.Transition);
            AddElement(110, 281, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 8)), 10, new bool[] { true }, new ushort[] { 281 }, "Darmstadtium", 7, new[] { 1.0 }, "Ds", ElementType.Transition);
            AddElement(111, 282, 'd', new ElectronConfiguration(ec, new Orbital(6, 'd', 9)), 11, new bool[] { true }, new ushort[] { 282 }, "Roentgenium", 7, new[] { 1.0 }, "Rg", ElementType.Transition);
            ec = new ElectronConfiguration(ec, new Orbital(6, 'd', 10));
            AddElement(112, 285, 'd', ec, 12, new bool[] { true }, new ushort[] { 285 }, "Copernicium", 7, new[] { 1.0 }, "Cn", ElementType.PostTransition);
            AddElement(113, 286, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 1)), 13, new bool[] { true }, new ushort[] { 286 }, "Nihonium", 7, new[] { 1.0 }, "Nh", ElementType.PostTransition);
            AddElement(114, 289, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 2)), 14, new bool[] { true }, new ushort[] { 289 }, "Flerovium", 7, new[] { 1.0 }, "Fl", ElementType.PostTransition);
            AddElement(115, 290, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 3)), 15, new bool[] { true }, new ushort[] { 290 }, "Moscovium", 7, new[] { 1.0 }, "Mc", ElementType.PostTransition | ElementType.Pnictogen);
            AddElement(116, 293, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 4)), 16, new bool[] { true }, new ushort[] { 293 }, "Livermorium", 7, new[] { 1.0 }, "Lv", ElementType.PostTransition | ElementType.Chalcogen);
            AddElement(117, 294, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 5)), 17, new bool[] { true }, new ushort[] { 294 }, "Tennessine", 7, new[] { 1.0 }, "Ts", ElementType.PostTransition | ElementType.Halogen);
            AddElement(118, 294, 'p', new ElectronConfiguration(ec, new Orbital(7, 'p', 6)), 18, new bool[] { true }, new ushort[] { 294 }, "Oganesson", 7, new[] { 1.0 }, "Og", ElementType.NobleGas);
        }

        private static void AddElement(
            byte atomicNumber,
            double averageMass,
            char block,
            ElectronConfiguration electronConfiguration,
            byte? group,
            bool[] isRadioactive,
            ushort[] massNumbers,
            string name,
            byte period,
            double[] relativeAbundances,
            string symbol,
            ElementType type)
        {
            var element = new Element(
                atomicNumber,
                averageMass,
                block,
                electronConfiguration,
                group,
                name,
                period,
                symbol,
                type);
            _ElementsByAtomicNumber[element.AtomicNumber] = element;
            _ElementsBySymbol[element.Symbol] = element;
            for (var i = 0; i < massNumbers.Length; i++)
            {
                var isotope = new Isotope(
                    element,
                    isRadioactive[i],
                    massNumbers[i],
                    relativeAbundances[i]);
                AddIsotopeInternal(isotope);
                if (i == 0)
                {
                    _CommonIsotopesByAtomicNumber[atomicNumber] = isotope;
                }
            }
        }
    }
}
