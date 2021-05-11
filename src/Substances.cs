using Tavenem.Chemistry.Elements;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Contains the substance registry, which provides lookup of known substances by id.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A variety of default substances included in the registry are also available as static
    /// instances on this class.
    /// </para>
    /// <para>
    /// The purpose of the registry is to allow keeping a reference, rather than a full
    /// <see cref="ISubstance"/> struct, for storage or transmission purposes, to reduce memory
    /// footprint and versioning issues.
    /// </para>
    /// <para>
    /// The <c>TryGetX(string)</c> methods will retireve substances from the registry by id.
    /// </para>
    /// <para>
    /// New substances can be added to the registry with the <c>Register(ISubstance)</c> method
    /// overloads.
    /// </para>
    /// <para>
    /// Substances can be stored in a location other than the registry. If there is a missed lookup
    /// for any substance name in the registry, the <c>TryGetX(string)</c> methods will attempt to
    /// fall back on the <see cref="DataStore"/>. Setting <see cref="AlwaysUseDataStore"/> to
    /// <see langword="true"/> will cause newly registered substances to be persisted to the <see
    /// cref="DataStore"/> as well.
    /// </para>
    /// </remarks>
    public static class Substances
    {
        private static readonly SemaphoreSlim _Lock = new(1);

        private static InMemoryDataStore _InternalDataStore = new();

        /// <summary>
        /// <para>
        /// If this has been set to <see langword="true"/>, the internal <see
        /// cref="InMemoryDataStore"/> will not be used to look up <see cref="ISubstance"/>s.
        /// </para>
        /// <para>
        /// This is set to <see langword="true"/> automatically when calling <see
        /// cref="All.RegisterAll(IDataStore)"/>. It may also be useful to set it to <see
        /// langword="true"/> by hand during your initialization procedures if you have previously
        /// primed your <see cref="IDataStore"/> backing store with the included substances (or with
        /// your own set intended to replace them).
        /// </para>
        /// </summary>
        public static bool AlwaysUseDataStore { get; set; }

        private static IDataStore _DataStore = _InternalDataStore;
        /// <summary>
        /// <para>
        /// The <see cref="IDataStore"/> in which <see cref="ISubstance"/> instances will be stored.
        /// </para>
        /// <para>
        /// This defaults to an internal <see cref="InMemoryDataStore"/> instance.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that the default set of <see cref="ISubstance"/>s included with the library are
        /// maintained in the internal <see cref="InMemoryDataStore"/> regardless of the value of
        /// this property, unless <see cref="AlwaysUseDataStore"/> is <see langword="true"/>.
        /// </para>
        /// <para>
        /// In that case, the default set of <see cref="ISubstance"/>s are not available at all
        /// until manually registered with <see cref="All.RegisterAll(IDataStore)"/>.
        /// </para>
        /// </remarks>
        public static IDataStore DataStore
        {
            get => _DataStore;
            set
            {
                _Lock.Wait();
                _DataStore = value;
                _Lock.Release();
            }
        }

        /// <summary>
        /// <para>
        /// Stores the given <paramref name="substance"/> in the data store.
        /// </para>
        /// <para>
        /// Replaces any substance already stored with the same id.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ISubstance"/> to store.</typeparam>
        /// <param name="substance">An <see cref="ISubstance"/> instance to store.</param>
        /// <returns>
        /// The <see cref="ISubstance"/> instance.
        /// </returns>
        /// <remarks>
        /// No exception is thrown if the substance has already been stored. The existing substance
        /// is replaced.
        /// </remarks>
        public static T Register<T>(this T substance) where T : class, ISubstance
        {
            DataStore.StoreItem(substance);
            return substance;
        }

        /// <summary>
        /// <para>
        /// Stores the given <paramref name="substance"/> in the given data store.
        /// </para>
        /// <para>
        /// Replaces any substance already stored with the same id.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ISubstance"/> to store.</typeparam>
        /// <param name="substance">An <see cref="ISubstance"/> instance to store.</param>
        /// <param name="dataStore">
        /// The <see cref="IDataStore"/> in which to store the <paramref name="substance"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ISubstance"/> instance.
        /// </returns>
        /// <remarks>
        /// No exception is thrown if the substance has already been stored. The existing substance
        /// is replaced.
        /// </remarks>
        public static T Register<T>(this T substance, IDataStore dataStore) where T : class, ISubstance
        {
            dataStore.StoreItem(substance);
            return substance;
        }

        /// <summary>
        /// <para>
        /// Attempts to retrieve the <see cref="ISubstance"/> with the given <paramref name="id"/>
        /// from the registry.
        /// </para>
        /// <para>
        /// If the registry has not yet been initialized, loads it with all pre-defined substances.
        /// </para>
        /// </summary>
        /// <param name="id">The id of the substance to be retrieved.</param>
        /// <param name="substance">When the method returns, will be set to the retrieved
        /// substance.</param>
        /// <returns><see langword="true"/> if a substance with the given <paramref name="id"/>
        /// was found in the registry; otherwise <see langword="false"/>.</returns>
        public static bool TryGetSubstance(string id, [NotNullWhen(true)] out ISubstance? substance)
        {
            if (string.IsNullOrEmpty(id))
            {
                substance = null;
                return false;
            }
            if (!AlwaysUseDataStore && _InternalDataStore.GetItem<ISubstance>(id) is ISubstance internalResult)
            {
                substance = internalResult;
                return true;
            }
            if (DataStore.GetItem<ISubstance>(id) is ISubstance result)
            {
                substance = result;
                return true;
            }
            substance = null;
            return false;
        }

        /// <summary>
        /// <para>
        /// Attempts to retrieve the <see cref="ISubstance"/> with the given <paramref name="id"/>
        /// from the registry.
        /// </para>
        /// <para>
        /// If the registry has not yet been initialized, loads it with all pre-defined substances.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ISubstance"/> to retrieve.</typeparam>
        /// <param name="id">The id of the substance to be retrieved.</param>
        /// <param name="substance">When the method returns, will be set to the retrieved
        /// substance.</param>
        /// <returns><see langword="true"/> if a substance with the given <paramref name="id"/>
        /// was found in the registry; otherwise <see langword="false"/>.</returns>
        public static bool TryGetSubstance<T>(string id, [NotNullWhen(true)] out T? substance) where T : class, ISubstance
        {
            if (string.IsNullOrEmpty(id))
            {
                substance = null;
                return false;
            }
            if (!AlwaysUseDataStore && _InternalDataStore.GetItem<T>(id) is T internalResult)
            {
                substance = internalResult;
                return true;
            }
            if (DataStore.GetItem<T>(id) is T result)
            {
                substance = result;
                return true;
            }
            substance = null;
            return false;
        }

        /// <summary>
        /// The collection of all predefined substances.
        /// </summary>
        public static class All
        {
            #region Chemicals

            #region Atmospheric

            /// <summary>
            /// ammonia
            /// </summary>
            public static readonly Chemical Ammonia = new(
                Formula.Parse("NH3"),
                "Ammonia",
                antoineCoefficientA: 3.18757,
                antoineCoefficientB: 506.713,
                antoineCoefficientC: -80.78,
                antoineMaximumTemperature: 239.6,
                antoineMinimumTemperature: 164.0,
                densityLiquid: 681.9,
                densitySolid: 817,
                isFlammable: true,
                meltingPoint: 195.42);

            /// <summary>
            /// ammonium hydrosulfide
            /// </summary>
            public static readonly Chemical AmmoniumHydrosulfide = new(
                Formula.Parse("(NH4)HS"),
                "Ammonium Hydrosulfide",
                antoineCoefficientA: 6.09146,
                antoineCoefficientB: 1598.378,
                antoineCoefficientC: -43.805,
                antoineMaximumTemperature: 306.4,
                antoineMinimumTemperature: 222.1,
                densityLiquid: 1170,
                densitySolid: 1170,
                isFlammable: true,
                meltingPoint: 329.8);

            /// <summary>
            /// hydrogen sulfide
            /// </summary>
            public static readonly Chemical HydrogenSulfide = new(
                Formula.Parse("H2S"),
                "Hydrogen Sulfide",
                antoineCoefficientA: 4.52887,
                antoineCoefficientB: 958.587,
                antoineCoefficientC: -0.539,
                antoineMaximumTemperature: 349.5,
                antoineMinimumTemperature: 212.8,
                densityLiquid: 993,
                densitySolid: 1120,
                isFlammable: true,
                meltingPoint: 191.15);

            /// <summary>
            /// phosphine
            /// </summary>
            public static readonly Chemical Phosphine = new(
                Formula.Parse("PH3"),
                "Phosphine",
                antoineCoefficientA: 4.02591,
                antoineCoefficientB: 702.651,
                antoineCoefficientC: -11.065,
                antoineMaximumTemperature: 185.7,
                antoineMinimumTemperature: 143.8,
                densityLiquid: 568.7,
                densitySolid: 568.7,
                isFlammable: true,
                meltingPoint: 140.35);

            /// <summary>
            /// sulphur dioxide
            /// </summary>
            public static readonly Chemical SulphurDioxide = new(
                Formula.Parse("SO2"),
                "Sulphur Dioxide",
                antoineCoefficientA: 4.40718,
                antoineCoefficientB: 999.90,
                antoineCoefficientC: -35.96,
                antoineMaximumTemperature: 279.5,
                antoineMinimumTemperature: 210.0,
                densityLiquid: 1377,
                densitySolid: 1377,
                meltingPoint: 202.15);

            #endregion Atmospheric

            #region Elements

            /// <summary>
            /// the native form of the element Hydrogen
            /// </summary>
            public static readonly Chemical Hydrogen = new(
                new Formula((PeriodicTable.Instance[1], 2)),
                "Hydrogen",
                antoineCoefficientA: 3.54314,
                antoineCoefficientB: 99.395,
                antoineCoefficientC: 7.726,
                antoineMaximumTemperature: 32.27,
                antoineMinimumTemperature: 21.01,
                densityLiquid: 70,
                densitySolid: 70,
                isFlammable: true,
                meltingPoint: 14.01);

            /// <summary>
            /// metallic Hydrogen
            /// </summary>
            public static readonly Chemical MetallicHydrogen = new(
                new Formula((PeriodicTable.Instance[1], 1)),
                "Metallic Hydrogen",
                antoineCoefficientA: 3.54314,
                antoineCoefficientB: 99.395,
                antoineCoefficientC: 7.726,
                antoineMaximumTemperature: 32.27,
                antoineMinimumTemperature: 21.01,
                densityLiquid: 600,
                densitySolid: 600,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 14.01);

            /// <summary>
            /// the native form of the element Helium
            /// </summary>
            public static readonly Chemical Helium = new(
                new Formula(PeriodicTable.Instance[2]),
                "Helium",
                antoineMaximumTemperature: 0,
                densityLiquid: 145,
                densitySolid: 145,
                meltingPoint: 0.95);

            /// <summary>
            /// the native form of the element Lithium
            /// </summary>
            public static readonly Chemical Lithium = new(
                new Formula(PeriodicTable.Instance[3]),
                "Lithium",
                antoineCoefficientA: 4.98831,
                antoineCoefficientB: 7918.984,
                antoineCoefficientC: -9.52,
                antoineMaximumTemperature: 1599.99,
                antoineMinimumTemperature: 298.14,
                densityLiquid: 512,
                densitySolid: 534,
                hardness: 5,
                isFlammable: true,
                meltingPoint: 453.65,
                youngsModulus: 4.91);

            /// <summary>
            /// the native form of the element Beryllium
            /// </summary>
            public static readonly Chemical Beryllium = new(
                new Formula(PeriodicTable.Instance[4]),
                "Beryllium",
                antoineMaximumTemperature: 2742,
                antoineMinimumTemperature: 2742,
                densityLiquid: 1690,
                densitySolid: 1850,
                hardness: 955,
                isFlammable: true,
                meltingPoint: 1560,
                youngsModulus: 318);

            /// <summary>
            /// the native form of the element Boron
            /// </summary>
            public static readonly Chemical Boron = new(
                new Formula(PeriodicTable.Instance[5]),
                "Boron",
                antoineMaximumTemperature: 4200,
                antoineMinimumTemperature: 4200,
                densityLiquid: 2080,
                densitySolid: 2080,
                hardness: 4900,
                meltingPoint: 2349,
                youngsModulus: 440);

            /// <summary>
            /// the amorphous allotrope form of the element Carbon
            /// </summary>
            public static readonly Chemical AmorphousCarbon = new(
                new Formula(PeriodicTable.Instance[6]),
                "Amorphous Carbon",
                antoineMaximumTemperature: 3915,
                antoineMinimumTemperature: 3915,
                densityLiquid: 1950,
                densitySolid: 1950,
                hardness: 310,
                isFlammable: true,
                meltingPoint: 3915,
                youngsModulus: 15.85);

            /// <summary>
            /// the diamond allotrope of the element Carbon
            /// </summary>
            public static readonly Chemical Diamond = new(
                new Formula(PeriodicTable.Instance[6]),
                "Diamond",
                antoineMaximumTemperature: 3915,
                antoineMinimumTemperature: 3915,
                densityLiquid: 3515,
                densitySolid: 3515,
                hardness: 45500,
                isGemstone: true,
                meltingPoint: 3915,
                youngsModulus: 1220);

            /// <summary>
            /// the native form of the element Nitrogen
            /// </summary>
            public static readonly Chemical Nitrogen = new(
                new Formula((PeriodicTable.Instance[7], 2)),
                "Nitrogen",
                antoineCoefficientA: 3.61947,
                antoineCoefficientB: 255.68,
                antoineCoefficientC: -6.6,
                antoineMaximumTemperature: 83.7,
                antoineMinimumTemperature: 63.2,
                densityLiquid: 808,
                densitySolid: 808,
                meltingPoint: 63.15);

            /// <summary>
            /// the native form of the element Oxygen
            /// </summary>
            public static readonly Chemical Oxygen = new(
                new Formula((PeriodicTable.Instance[8], 2)),
                "Oxygen",
                antoineCoefficientA: 3.81634,
                antoineCoefficientB: 319.01,
                antoineCoefficientC: -6.453,
                antoineMaximumTemperature: 97.2,
                antoineMinimumTemperature: 62.6,
                densityLiquid: 1141,
                densitySolid: 1141,
                // Oxygen is not really flammable: it's an oxidizer; but the difference in practice
                // is considered unimportant for this library's purposes.
                isFlammable: true,
                meltingPoint: 54.36);

            /// <summary>
            /// the ozone form of the element Oxygen
            /// </summary>
            public static readonly Chemical Ozone = new(
                new Formula((PeriodicTable.Instance[8], 3)),
                "Ozone",
                antoineCoefficientA: 4.23637,
                antoineCoefficientB: 712.487,
                antoineCoefficientC: 6.982,
                antoineMaximumTemperature: 162.0,
                antoineMinimumTemperature: 92.8,
                densityLiquid: 1354,
                densitySolid: 1354,
                // Like oxygen, ozone is not really flammable, but an oxidizer.
                isFlammable: true,
                meltingPoint: 81.15);

            /// <summary>
            /// the native form of the element Fluorine
            /// </summary>
            public static readonly Chemical Fluorine = new(
                new Formula((PeriodicTable.Instance[9], 2)),
                "Fluorine",
                antoineCoefficientA: 4.02355,
                antoineCoefficientB: 322.067,
                antoineCoefficientC: -4.748,
                antoineMaximumTemperature: 143.99,
                antoineMinimumTemperature: 53.99,
                densityLiquid: 1696,
                densitySolid: 1696,
                // Like oxygen, fluorine is not really flammable, but an oxidizer.
                isFlammable: true,
                meltingPoint: 53.48);

            /// <summary>
            /// the native form of the element Neon
            /// </summary>
            public static readonly Chemical Neon = new(
                new Formula(PeriodicTable.Instance[10]),
                "Neon",
                antoineCoefficientA: 3.75641,
                antoineCoefficientB: 95.599,
                antoineCoefficientC: -1.503,
                antoineMaximumTemperature: 27.0,
                antoineMinimumTemperature: 15.9,
                densityLiquid: 1207,
                densitySolid: 1207,
                meltingPoint: 24.55);

            /// <summary>
            /// the native form of the element Sodium
            /// </summary>
            public static readonly Chemical Sodium = new(
                new Formula(PeriodicTable.Instance[11]),
                "Sodium",
                antoineCoefficientA: 2.46077,
                antoineCoefficientB: 1873.728,
                antoineCoefficientC: -416.372,
                antoineMaximumTemperature: 1118.0,
                antoineMinimumTemperature: 924,
                densityLiquid: 927,
                densitySolid: 968,
                hardness: 0.69,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 370.944,
                youngsModulus: 6.8);

            /// <summary>
            /// the native form of the element Magnesium
            /// </summary>
            public static readonly Chemical Magnesium = new(
                new Formula(PeriodicTable.Instance[12]),
                "Magnesium",
                antoineMaximumTemperature: 1363,
                antoineMinimumTemperature: 1363,
                densityLiquid: 1584,
                densitySolid: 1738,
                hardness: 152,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 370.944,
                youngsModulus: 44.7);

            /// <summary>
            /// the native form of the element Aluminium
            /// </summary>
            public static readonly Chemical Aluminium = new(
                new Formula(PeriodicTable.Instance[13]),
                "Aluminium",
                antoineCoefficientA: 5.73623,
                antoineCoefficientB: 13204.109,
                antoineCoefficientC: -24.306,
                antoineMaximumTemperature: 2329.0,
                antoineMinimumTemperature: 1557.0,
                densityLiquid: 2375,
                densitySolid: 2700,
                hardness: 255,
                isConductive: true,
                meltingPoint: 933.45,
                youngsModulus: 70.2);

            /// <summary>
            /// the native form of the element Silicon
            /// </summary>
            public static readonly Chemical Silicon = new(
                new Formula(PeriodicTable.Instance[14]),
                "Silicon",
                antoineCoefficientA: 9.56436,
                antoineCoefficientB: 23308.848,
                antoineCoefficientC: -123.133,
                antoineMaximumTemperature: 2560.0,
                antoineMinimumTemperature: 1997.0,
                densityLiquid: 2570,
                densitySolid: 2329,
                hardness: 1224,
                meltingPoint: 1687,
                youngsModulus: 113);

            /// <summary>
            /// the white allotrope of the element Phosphorus
            /// </summary>
            public static readonly Chemical WhitePhosphorus = new(
                new Formula((PeriodicTable.Instance[15], 4)),
                "White Phosphorus",
                antoineCoefficientA: 5.04162,
                antoineCoefficientB: 2819.239,
                antoineCoefficientC: 6.399,
                antoineMaximumTemperature: 553,
                antoineMinimumTemperature: 349.8,
                densityLiquid: 1823,
                densitySolid: 1823,
                isFlammable: true,
                meltingPoint: 317.3,
                youngsModulus: 30.4);

            /// <summary>
            /// the red allotrope of the element Phosphorus
            /// </summary>
            public static readonly Chemical RedPhosphorus = new(
                new Formula(PeriodicTable.Instance[15]),
                "Red Phosphorus",
                antoineCoefficientA: 5.04162,
                antoineCoefficientB: 2819.239,
                antoineCoefficientC: 6.399,
                antoineMaximumTemperature: 553,
                antoineMinimumTemperature: 349.8,
                densityLiquid: 2270,
                densitySolid: 2270,
                isFlammable: true,
                meltingPoint: 860,
                youngsModulus: 30.4);

            /// <summary>
            /// the native form of the element Sulfur
            /// </summary>
            public static readonly Chemical Sulfur = new(
                new Formula((PeriodicTable.Instance[16], 8)),
                "Sulfur",
                antoineMaximumTemperature: 717.8,
                antoineMinimumTemperature: 717.8,
                densityLiquid: 1819,
                densitySolid: 1960,
                hardness: 16,
                isFlammable: true,
                meltingPoint: 388.36,
                youngsModulus: 17.8);

            /// <summary>
            /// the native form of the element Chlorine
            /// </summary>
            public static readonly Chemical Chlorine = new(
                new Formula((PeriodicTable.Instance[17], 2)),
                "Chlorine",
                antoineCoefficientA: 3.0213,
                antoineCoefficientB: 530.591,
                antoineCoefficientC: -64.639,
                antoineMaximumTemperature: 239.4,
                antoineMinimumTemperature: 155,
                densityLiquid: 1562.5,
                densitySolid: 1562.5,
                // Like oxygen, chlorine is not really flammable, but an oxidizer.
                isFlammable: true,
                meltingPoint: 171.6);

            /// <summary>
            /// the native form of the element Argon
            /// </summary>
            public static readonly Chemical Argon = new(
                new Formula(PeriodicTable.Instance[18]),
                "Argon",
                antoineCoefficientA: 3.29555,
                antoineCoefficientB: 215.24,
                antoineCoefficientC: -22.233,
                antoineMaximumTemperature: 150.72,
                antoineMinimumTemperature: 83.78,
                densityLiquid: 1395.4,
                densitySolid: 1395.4,
                meltingPoint: 83.8);

            /// <summary>
            /// the native form of the element Potassium
            /// </summary>
            public static readonly Chemical Potassium = new(
                new Formula(PeriodicTable.Instance[19]),
                "Potassium",
                antoineCoefficientA: 4.45718,
                antoineCoefficientB: 4691.58,
                antoineCoefficientC: 24.195,
                antoineMaximumTemperature: 1033.0,
                antoineMinimumTemperature: 679.4,
                densityLiquid: 828,
                densitySolid: 862,
                hardness: 0.363,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 336.7,
                youngsModulus: 3.175);

            /// <summary>
            /// the native form of the element Calcium
            /// </summary>
            public static readonly Chemical Calcium = new(
                new Formula(PeriodicTable.Instance[20]),
                "Calcium",
                antoineCoefficientA: 2.78473,
                antoineCoefficientB: 3121.368,
                antoineCoefficientC: -594.591,
                antoineMaximumTemperature: 1712,
                antoineMinimumTemperature: 1254,
                densityLiquid: 1378,
                densitySolid: 1550,
                hardness: 293,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 1115,
                youngsModulus: 19.6);

            /// <summary>
            /// the native form of the element Scandium
            /// </summary>
            public static readonly Chemical Scandium = new(
                new Formula(PeriodicTable.Instance[21]),
                "Scandium",
                antoineMaximumTemperature: 3109,
                antoineMinimumTemperature: 3109,
                densityLiquid: 2800,
                densitySolid: 2985,
                hardness: 968,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 1814,
                youngsModulus: 74.4);

            /// <summary>
            /// the native form of the element Titanium
            /// </summary>
            public static readonly Chemical Titanium = new(
                new Formula(PeriodicTable.Instance[22]),
                "Titanium",
                antoineMaximumTemperature: 3560,
                antoineMinimumTemperature: 3560,
                densityLiquid: 5500,
                densitySolid: 6000,
                hardness: 2125,
                isConductive: true,
                meltingPoint: 1941,
                youngsModulus: 120.2);

            /// <summary>
            /// the native form of the element Vanadium
            /// </summary>
            public static readonly Chemical Vanadium = new(
                new Formula(PeriodicTable.Instance[23]),
                "Vanadium",
                antoineMaximumTemperature: 3680,
                antoineMinimumTemperature: 3680,
                densityLiquid: 4110,
                densitySolid: 4506,
                hardness: 671,
                isConductive: true,
                meltingPoint: 2183,
                youngsModulus: 127.6);

            /// <summary>
            /// the native form of the element Chromium
            /// </summary>
            public static readonly Chemical Chromium = new(
                new Formula(PeriodicTable.Instance[24]),
                "Chromium",
                antoineCoefficientA: 6.02371,
                antoineCoefficientB: 16064.989,
                antoineCoefficientC: -83.86,
                antoineMaximumTemperature: 2755,
                antoineMinimumTemperature: 1889,
                densityLiquid: 6300,
                densitySolid: 7190,
                hardness: 1060,
                isConductive: true,
                meltingPoint: 2180,
                youngsModulus: 279);

            /// <summary>
            /// the native form of the element Manganese
            /// </summary>
            public static readonly Chemical Manganese = new(
                new Formula(PeriodicTable.Instance[25]),
                "Manganese",
                antoineMaximumTemperature: 2334,
                antoineMinimumTemperature: 2334,
                densityLiquid: 5950,
                densitySolid: 7210,
                hardness: 196,
                isConductive: true,
                meltingPoint: 1519,
                youngsModulus: 191);

            /// <summary>
            /// the native form of the element Iron
            /// </summary>
            public static readonly Chemical Iron = new(
                new Formula(PeriodicTable.Instance[26]),
                "Iron",
                antoineMaximumTemperature: 3134,
                antoineMinimumTemperature: 3134,
                densityLiquid: 6980,
                densitySolid: 7874,
                hardness: 608,
                isConductive: true,
                meltingPoint: 1811.15,
                youngsModulus: 208.2);

            /// <summary>
            /// the native form of the element Cobalt
            /// </summary>
            public static readonly Chemical Cobalt = new(
                new Formula(PeriodicTable.Instance[27]),
                "Cobalt",
                antoineMaximumTemperature: 3200,
                antoineMinimumTemperature: 3200,
                densityLiquid: 8860,
                densitySolid: 8900,
                hardness: 1043,
                isConductive: true,
                meltingPoint: 1768,
                youngsModulus: 211);

            /// <summary>
            /// the native form of the element Nickel
            /// </summary>
            public static readonly Chemical Nickel = new(
                new Formula(PeriodicTable.Instance[28]),
                "Nickel",
                antoineCoefficientA: 5.98183,
                antoineCoefficientB: 16808.435,
                antoineCoefficientC: -188.717,
                antoineMaximumTemperature: 3005,
                antoineMinimumTemperature: 2083,
                densityLiquid: 7810,
                densitySolid: 8908,
                hardness: 638,
                isConductive: true,
                meltingPoint: 1728.15,
                youngsModulus: 199.5);

            /// <summary>
            /// the native form of the element Copper
            /// </summary>
            public static readonly Chemical Copper = new(
                new Formula(PeriodicTable.Instance[29]),
                "Copper",
                antoineMaximumTemperature: 2835,
                antoineMinimumTemperature: 2835,
                densityLiquid: 8020,
                densitySolid: 8960,
                hardness: 356,
                isConductive: true,
                meltingPoint: 1357.95,
                youngsModulus: 129.8);

            /// <summary>
            /// the native form of the element Zinc
            /// </summary>
            public static readonly Chemical Zinc = new(
                new Formula(PeriodicTable.Instance[30]),
                "Zinc",
                antoineMaximumTemperature: 1180,
                antoineMinimumTemperature: 1180,
                densityLiquid: 6570,
                densitySolid: 7140,
                hardness: 369.5,
                isConductive: true,
                meltingPoint: 692.68,
                youngsModulus: 104.5);

            /// <summary>
            /// the native form of the element Gallium
            /// </summary>
            public static readonly Chemical Gallium = new(
                new Formula(PeriodicTable.Instance[31]),
                "Gallium",
                antoineMaximumTemperature: 2673,
                antoineMinimumTemperature: 2673,
                densityLiquid: 6095,
                densitySolid: 5910,
                hardness: 62.75,
                isConductive: true,
                meltingPoint: 302.9146,
                youngsModulus: 9.81);

            /// <summary>
            /// the native form of the element Germanium
            /// </summary>
            public static readonly Chemical Germanium = new(
                new Formula(PeriodicTable.Instance[32]),
                "Germanium",
                antoineMaximumTemperature: 3106,
                antoineMinimumTemperature: 3106,
                densityLiquid: 5600,
                densitySolid: 5323,
                hardness: 2125,
                meltingPoint: 1211.40,
                youngsModulus: 79.9);

            /// <summary>
            /// the native form of the element Arsenic
            /// </summary>
            public static readonly Chemical Arsenic = new(
                new Formula((PeriodicTable.Instance[33], 6)),
                "Arsenic",
                antoineMaximumTemperature: 887,
                antoineMinimumTemperature: 887,
                densityLiquid: 5220,
                densitySolid: 5727,
                hardness: 1440,
                meltingPoint: 887,
                youngsModulus: 22);

            /// <summary>
            /// the native form of the element Selenium
            /// </summary>
            public static readonly Chemical Selenium = new(
                new Formula(PeriodicTable.Instance[34]),
                "Selenium",
                antoineCoefficientA: 6.33714,
                antoineCoefficientB: 6588.125,
                antoineCoefficientC: 86.633,
                antoineMaximumTemperature: 953,
                antoineMinimumTemperature: 629,
                densityLiquid: 3990,
                densitySolid: 4810,
                hardness: 736,
                meltingPoint: 494,
                youngsModulus: 58);

            /// <summary>
            /// the native form of the element Bromine
            /// </summary>
            public static readonly Chemical Bromine = new(
                new Formula((PeriodicTable.Instance[35], 2)),
                "Bromine",
                antoineCoefficientA: 2.94529,
                antoineCoefficientB: 638.258,
                antoineCoefficientC: -115.144,
                antoineMaximumTemperature: 331.4,
                antoineMinimumTemperature: 224.5,
                densityLiquid: 3102.8,
                meltingPoint: 265.8);

            /// <summary>
            /// the native form of the element Krypton
            /// </summary>
            public static readonly Chemical Krypton = new(
                new Formula(PeriodicTable.Instance[36]),
                "Krypton",
                antoineCoefficientA: 4.2064,
                antoineCoefficientB: 539.004,
                antoineCoefficientC: 8.855,
                antoineMaximumTemperature: 208.0,
                antoineMinimumTemperature: 126.68,
                densityLiquid: 2413,
                meltingPoint: 115.75);

            /// <summary>
            /// the native form of the element Rubidium
            /// </summary>
            public static readonly Chemical Rubidium = new(
                new Formula(PeriodicTable.Instance[37]),
                "Rubidium",
                antoineMaximumTemperature: 961,
                antoineMinimumTemperature: 961,
                densityLiquid: 1460,
                densitySolid: 1532,
                hardness: 0.216,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 312.45,
                youngsModulus: 2.35);

            /// <summary>
            /// the native form of the element Strontium
            /// </summary>
            public static readonly Chemical Strontium = new(
                new Formula(PeriodicTable.Instance[38]),
                "Strontium",
                antoineMaximumTemperature: 1650,
                antoineMinimumTemperature: 1650,
                densityLiquid: 2375,
                densitySolid: 2640,
                hardness: 62.75,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1050,
                youngsModulus: 15.7);

            /// <summary>
            /// the native form of the element Yttrium
            /// </summary>
            public static readonly Chemical Yttrium = new(
                new Formula(PeriodicTable.Instance[39]),
                "Yttrium",
                antoineMaximumTemperature: 3203,
                antoineMinimumTemperature: 3203,
                densityLiquid: 4240,
                densitySolid: 4472,
                hardness: 394.5,
                isConductive: true,
                meltingPoint: 1799,
                youngsModulus: 63.5);

            /// <summary>
            /// the native form of the element Zirconium
            /// </summary>
            public static readonly Chemical Zirconium = new(
                new Formula(PeriodicTable.Instance[40]),
                "Zirconium",
                antoineMaximumTemperature: 4650,
                antoineMinimumTemperature: 4650,
                densityLiquid: 5800,
                densitySolid: 6520,
                hardness: 1310,
                isConductive: true,
                meltingPoint: 2128,
                youngsModulus: 97.1);

            /// <summary>
            /// the native form of the element Niobium
            /// </summary>
            public static readonly Chemical Niobium = new(
                new Formula(PeriodicTable.Instance[41]),
                "Niobium",
                antoineMaximumTemperature: 5017,
                antoineMinimumTemperature: 5017,
                densitySolid: 8570,
                hardness: 1095,
                isConductive: true,
                meltingPoint: 2750,
                youngsModulus: 104.9);

            /// <summary>
            /// the native form of the element Molybdenum
            /// </summary>
            public static readonly Chemical Molybdenum = new(
                new Formula(PeriodicTable.Instance[42]),
                "Molybdenum",
                antoineMaximumTemperature: 4912,
                antoineMinimumTemperature: 4912,
                densityLiquid: 9330,
                densitySolid: 10280,
                hardness: 2070,
                isConductive: true,
                meltingPoint: 2896,
                youngsModulus: 324.8);

            /// <summary>
            /// the native form of the element Technetium
            /// </summary>
            public static readonly Chemical Technetium = new(
                new Formula(PeriodicTable.Instance[43]),
                "Technetium",
                antoineMaximumTemperature: 4538,
                antoineMinimumTemperature: 4538,
                densitySolid: 11000,
                isConductive: true,
                meltingPoint: 2430,
                youngsModulus: 407);

            /// <summary>
            /// the native form of the element Ruthenium
            /// </summary>
            public static readonly Chemical Ruthenium = new(
                new Formula(PeriodicTable.Instance[44]),
                "Ruthenium",
                antoineMaximumTemperature: 4423,
                antoineMinimumTemperature: 4423,
                densityLiquid: 10650,
                densitySolid: 12450,
                hardness: 2160,
                isConductive: true,
                meltingPoint: 2607,
                youngsModulus: 432);

            /// <summary>
            /// the native form of the element Rhodium
            /// </summary>
            public static readonly Chemical Rhodium = new(
                new Formula(PeriodicTable.Instance[45]),
                "Rhodium",
                antoineMaximumTemperature: 3968,
                antoineMinimumTemperature: 3968,
                densityLiquid: 10700,
                densitySolid: 12410,
                hardness: 1225,
                isConductive: true,
                meltingPoint: 2237,
                youngsModulus: 379);

            /// <summary>
            /// the native form of the element Palladium
            /// </summary>
            public static readonly Chemical Palladium = new(
                new Formula(PeriodicTable.Instance[46]),
                "Palladium",
                antoineMaximumTemperature: 3236,
                antoineMinimumTemperature: 3236,
                densityLiquid: 10380,
                densitySolid: 12023,
                hardness: 500,
                isConductive: true,
                meltingPoint: 1828.05,
                youngsModulus: 121);

            /// <summary>
            /// the native form of the element Silver
            /// </summary>
            public static readonly Chemical Silver = new(
                new Formula(PeriodicTable.Instance[47]),
                "Silver",
                antoineCoefficientA: 1.95303,
                antoineCoefficientB: 2505.533,
                antoineCoefficientC: -1194.947,
                antoineMaximumTemperature: 2425.0,
                antoineMinimumTemperature: 1823.0,
                densityLiquid: 9320,
                densitySolid: 10490,
                hardness: 251,
                isConductive: true,
                meltingPoint: 1234.95,
                youngsModulus: 82.7);

            /// <summary>
            /// the native form of the element Cadmium
            /// </summary>
            public static readonly Chemical Cadmium = new(
                new Formula(PeriodicTable.Instance[48]),
                "Cadmium",
                antoineMaximumTemperature: 1040,
                antoineMinimumTemperature: 1040,
                densityLiquid: 7996,
                densitySolid: 8650,
                hardness: 211.5,
                isConductive: true,
                meltingPoint: 594.22,
                youngsModulus: 62.6);

            /// <summary>
            /// the native form of the element Indium
            /// </summary>
            public static readonly Chemical Indium = new(
                new Formula(PeriodicTable.Instance[49]),
                "Indium",
                antoineMaximumTemperature: 2345,
                antoineMinimumTemperature: 2345,
                densityLiquid: 7020,
                densitySolid: 7310,
                hardness: 9.4,
                isConductive: true,
                meltingPoint: 429.7485,
                youngsModulus: 10.6);

            /// <summary>
            /// the white allotrope of the element Tin
            /// </summary>
            public static readonly Chemical WhiteTin = new(
                new Formula(PeriodicTable.Instance[50]),
                "White Tin",
                antoineCoefficientA: 6.59594,
                antoineCoefficientB: 16866.811,
                antoineCoefficientC: 15.465,
                antoineMaximumTemperature: 2543.0,
                antoineMinimumTemperature: 1765.0,
                densityLiquid: 6990,
                densitySolid: 7265,
                hardness: 245,
                isConductive: true,
                meltingPoint: 505.08,
                youngsModulus: 49.9);

            /// <summary>
            /// the gray allotrope of the element Tin
            /// </summary>
            public static readonly Chemical GrayTin = new(
                new Formula((PeriodicTable.Instance[50], 8)),
                "Gray Tin",
                antoineCoefficientA: 6.59594,
                antoineCoefficientB: 16866.811,
                antoineCoefficientC: 15.465,
                antoineMaximumTemperature: 2543.0,
                antoineMinimumTemperature: 1765.0,
                densityLiquid: 6990,
                densitySolid: 5769,
                hardness: 245,
                meltingPoint: 505.08);

            /// <summary>
            /// the native form of the element Antimony
            /// </summary>
            public static readonly Chemical Antimony = new(
                new Formula((PeriodicTable.Instance[51], 6)),
                "Antimony",
                antoineCoefficientA: 2.26041,
                antoineCoefficientB: 4475.449,
                antoineCoefficientC: -152.352,
                antoineMaximumTemperature: 1373,
                antoineMinimumTemperature: 1058,
                densityLiquid: 6530,
                densitySolid: 6697,
                hardness: 339,
                isConductive: true,
                meltingPoint: 903.78,
                youngsModulus: 54.7);

            /// <summary>
            /// the native form of the element Tellurium
            /// </summary>
            public static readonly Chemical Tellurium = new(
                new Formula(PeriodicTable.Instance[52]),
                "Tellurium",
                antoineMaximumTemperature: 1261,
                antoineMinimumTemperature: 1261,
                densityLiquid: 5700,
                densitySolid: 6240,
                hardness: 225,
                meltingPoint: 722.66,
                youngsModulus: 47.1);

            /// <summary>
            /// the native form of the element Iodine
            /// </summary>
            public static readonly Chemical Iodine = new(
                new Formula((PeriodicTable.Instance[53], 2)),
                "Iodine",
                antoineCoefficientA: 3.36429,
                antoineCoefficientB: 1039.159,
                antoineCoefficientC: -146.589,
                antoineMaximumTemperature: 456,
                antoineMinimumTemperature: 311.9,
                densityLiquid: 3960,
                densitySolid: 4933,
                meltingPoint: 386.85);

            /// <summary>
            /// the native form of the element Xenon
            /// </summary>
            public static readonly Chemical Xenon = new(
                new Formula(PeriodicTable.Instance[54]),
                "Xenon",
                antoineCoefficientA: 3.80675,
                antoineCoefficientB: 577.661,
                antoineCoefficientC: -13.0,
                antoineMaximumTemperature: 184.70,
                antoineMinimumTemperature: 161.70,
                densityLiquid: 2942,
                meltingPoint: 161.35);

            /// <summary>
            /// the native form of the element Caesium
            /// </summary>
            public static readonly Chemical Caesium = new(
                new Formula(PeriodicTable.Instance[55]),
                "Caesium",
                antoineCoefficientA: 3.69576,
                antoineCoefficientB: 3453.122,
                antoineCoefficientC: -26.829,
                antoineMaximumTemperature: 963,
                antoineMinimumTemperature: 552,
                densityLiquid: 1843,
                densitySolid: 1930,
                hardness: 0.14,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 301.7,
                youngsModulus: 1.69);

            /// <summary>
            /// the native form of the element Barium
            /// </summary>
            public static readonly Chemical Barium = new(
                new Formula(PeriodicTable.Instance[56]),
                "Barium",
                antoineCoefficientA: 4.08188,
                antoineCoefficientB: 7599.352,
                antoineCoefficientC: -45.737,
                antoineMaximumTemperature: 1911,
                antoineMinimumTemperature: 1257,
                densityLiquid: 1843,
                densitySolid: 1930,
                hardness: 10,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1000,
                youngsModulus: 12.8);

            /// <summary>
            /// the native form of the element Lanthanum
            /// </summary>
            public static readonly Chemical Lanthanum = new(
                new Formula(PeriodicTable.Instance[57]),
                "Lanthanum",
                antoineMaximumTemperature: 3737,
                antoineMinimumTemperature: 3737,
                densityLiquid: 5940,
                densitySolid: 6162,
                hardness: 375,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1193,
                youngsModulus: 36.6);

            /// <summary>
            /// the native form of the element Cerium
            /// </summary>
            public static readonly Chemical Cerium = new(
                new Formula(PeriodicTable.Instance[58]),
                "Cerium",
                antoineMaximumTemperature: 3716,
                antoineMinimumTemperature: 3716,
                densityLiquid: 6550,
                densitySolid: 6770,
                hardness: 311,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1068,
                youngsModulus: 33.6);

            /// <summary>
            /// the native form of the element Praseodymium
            /// </summary>
            public static readonly Chemical Praseodymium = new(
                new Formula(PeriodicTable.Instance[59]),
                "Praseodymium",
                antoineMaximumTemperature: 3403,
                antoineMinimumTemperature: 3403,
                densityLiquid: 6500,
                densitySolid: 6770,
                hardness: 445,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1208,
                youngsModulus: 37.3);

            /// <summary>
            /// the native form of the element Neodymium
            /// </summary>
            public static readonly Chemical Neodymium = new(
                new Formula(PeriodicTable.Instance[60]),
                "Neodymium",
                antoineMaximumTemperature: 3347,
                antoineMinimumTemperature: 3347,
                densityLiquid: 6890,
                densitySolid: 7010,
                hardness: 522.5,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1297,
                youngsModulus: 41.4);

            /// <summary>
            /// the native form of the element Promethium
            /// </summary>
            public static readonly Chemical Promethium = new(
                new Formula(PeriodicTable.Instance[61]),
                "Promethium",
                antoineMaximumTemperature: 3273,
                antoineMinimumTemperature: 3273,
                densitySolid: 7260,
                hardness: 617.8,
                isConductive: true,
                meltingPoint: 1315,
                youngsModulus: 46);

            /// <summary>
            /// the native form of the element Samarium
            /// </summary>
            public static readonly Chemical Samarium = new(
                new Formula(PeriodicTable.Instance[62]),
                "Samarium",
                antoineMaximumTemperature: 2173,
                antoineMinimumTemperature: 2173,
                densityLiquid: 7160,
                densitySolid: 7520,
                hardness: 440,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1345,
                youngsModulus: 49.7);

            /// <summary>
            /// the native form of the element Europium
            /// </summary>
            public static readonly Chemical Europium = new(
                new Formula(PeriodicTable.Instance[63]),
                "Europium",
                antoineMaximumTemperature: 1802,
                antoineMinimumTemperature: 1802,
                densityLiquid: 5130,
                densitySolid: 5264,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1099,
                youngsModulus: 18.2);

            /// <summary>
            /// the native form of the element Gadolinium
            /// </summary>
            public static readonly Chemical Gadolinium = new(
                new Formula(PeriodicTable.Instance[64]),
                "Gadolinium",
                antoineMaximumTemperature: 3273,
                antoineMinimumTemperature: 3273,
                densityLiquid: 7400,
                densitySolid: 7900,
                hardness: 730,
                isConductive: true,
                meltingPoint: 1585,
                youngsModulus: 54.8);

            /// <summary>
            /// the native form of the element Terbium
            /// </summary>
            public static readonly Chemical Terbium = new(
                new Formula(PeriodicTable.Instance[65]),
                "Terbium",
                antoineMaximumTemperature: 3396,
                antoineMinimumTemperature: 3396,
                densityLiquid: 7650,
                densitySolid: 8230,
                hardness: 770,
                isConductive: true,
                meltingPoint: 1629,
                youngsModulus: 55.7);

            /// <summary>
            /// the native form of the element Dysprosium
            /// </summary>
            public static readonly Chemical Dysprosium = new(
                new Formula(PeriodicTable.Instance[66]),
                "Dysprosium",
                antoineMaximumTemperature: 2840,
                antoineMinimumTemperature: 2840,
                densityLiquid: 8370,
                densitySolid: 8540,
                hardness: 525,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1680,
                youngsModulus: 61.4);

            /// <summary>
            /// the native form of the element Holmium
            /// </summary>
            public static readonly Chemical Holmium = new(
                new Formula(PeriodicTable.Instance[67]),
                "Holmium",
                antoineMaximumTemperature: 2873,
                antoineMinimumTemperature: 2873,
                densityLiquid: 8340,
                densitySolid: 8790,
                hardness: 550,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1734,
                youngsModulus: 64.8);

            /// <summary>
            /// the native form of the element Erbium
            /// </summary>
            public static readonly Chemical Erbium = new(
                new Formula(PeriodicTable.Instance[68]),
                "Erbium",
                antoineMaximumTemperature: 3141,
                antoineMinimumTemperature: 3141,
                densityLiquid: 8860,
                densitySolid: 9066,
                hardness: 650,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1802,
                youngsModulus: 69.9);

            /// <summary>
            /// the native form of the element Thulium
            /// </summary>
            public static readonly Chemical Thulium = new(
                new Formula(PeriodicTable.Instance[69]),
                "Thulium",
                antoineMaximumTemperature: 2223,
                antoineMinimumTemperature: 2223,
                densityLiquid: 8560,
                densitySolid: 9320,
                hardness: 560,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1818,
                youngsModulus: 74);

            /// <summary>
            /// the native form of the element Ytterbium
            /// </summary>
            public static readonly Chemical Ytterbium = new(
                new Formula(PeriodicTable.Instance[70]),
                "Ytterbium",
                antoineMaximumTemperature: 1469,
                antoineMinimumTemperature: 1469,
                densityLiquid: 6210,
                densitySolid: 6900,
                hardness: 295,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1097,
                youngsModulus: 23.9);

            /// <summary>
            /// the native form of the element Lutetium
            /// </summary>
            public static readonly Chemical Lutetium = new(
                new Formula(PeriodicTable.Instance[71]),
                "Lutetium",
                antoineMaximumTemperature: 3675,
                antoineMinimumTemperature: 3675,
                densityLiquid: 9300,
                densitySolid: 9841,
                hardness: 1025,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 1925,
                youngsModulus: 68.6);

            /// <summary>
            /// the native form of the element Hafnium
            /// </summary>
            public static readonly Chemical Hafnium = new(
                new Formula(PeriodicTable.Instance[72]),
                "Hafnium",
                antoineMaximumTemperature: 4876,
                antoineMinimumTemperature: 4876,
                densityLiquid: 12000,
                densitySolid: 13310,
                hardness: 1790,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 2506,
                youngsModulus: 141);

            /// <summary>
            /// the native form of the element Tantalum
            /// </summary>
            public static readonly Chemical Tantalum = new(
                new Formula(PeriodicTable.Instance[73]),
                "Tantalum",
                antoineMaximumTemperature: 5731,
                antoineMinimumTemperature: 5731,
                densityLiquid: 15000,
                densitySolid: 16690,
                hardness: 1035,
                isConductive: true,
                meltingPoint: 3290,
                youngsModulus: 185.7);

            /// <summary>
            /// the native form of the element Tungsten
            /// </summary>
            public static readonly Chemical Tungsten = new(
                new Formula(PeriodicTable.Instance[74]),
                "Tungsten",
                antoineMaximumTemperature: 6203,
                antoineMinimumTemperature: 6203,
                densityLiquid: 17600,
                densitySolid: 19300,
                hardness: 3715,
                isConductive: true,
                meltingPoint: 3695,
                youngsModulus: 411);

            /// <summary>
            /// the native form of the element Rhenium
            /// </summary>
            public static readonly Chemical Rhenium = new(
                new Formula(PeriodicTable.Instance[75]),
                "Rhenium",
                antoineMaximumTemperature: 5903,
                antoineMinimumTemperature: 5903,
                densityLiquid: 18900,
                densitySolid: 21020,
                hardness: 1925,
                isConductive: true,
                meltingPoint: 3459,
                youngsModulus: 520);

            /// <summary>
            /// the native form of the element Osmium
            /// </summary>
            public static readonly Chemical Osmium = new(
                new Formula(PeriodicTable.Instance[76]),
                "Osmium",
                antoineMaximumTemperature: 5285,
                antoineMinimumTemperature: 5285,
                densityLiquid: 20000,
                densitySolid: 22590,
                hardness: 296.5,
                isConductive: true,
                meltingPoint: 3306,
                youngsModulus: 558.6);

            /// <summary>
            /// the native form of the element Iridium
            /// </summary>
            public static readonly Chemical Iridium = new(
                new Formula(PeriodicTable.Instance[77]),
                "Iridium",
                antoineMaximumTemperature: 4403,
                antoineMinimumTemperature: 4403,
                densityLiquid: 19000,
                densitySolid: 22560,
                hardness: 1760,
                isConductive: true,
                meltingPoint: 2719,
                youngsModulus: 528);

            /// <summary>
            /// the native form of the element Platinum
            /// </summary>
            public static readonly Chemical Platinum = new(
                new Formula(PeriodicTable.Instance[78]),
                "Platinum",
                antoineCoefficientA: 4.80688,
                antoineCoefficientB: 21519.696,
                antoineCoefficientC: -200.689,
                antoineMaximumTemperature: 4680.0,
                antoineMinimumTemperature: 3003.0,
                densityLiquid: 19770,
                densitySolid: 21450,
                hardness: 450,
                isConductive: true,
                meltingPoint: 2041.15,
                youngsModulus: 172.4);

            /// <summary>
            /// the native form of the element Gold
            /// </summary>
            public static readonly Chemical Gold = new(
                new Formula(PeriodicTable.Instance[79]),
                "Gold",
                antoineCoefficientA: 5.46951,
                antoineCoefficientB: 17292.476,
                antoineCoefficientC: -70.978,
                antoineMaximumTemperature: 3239.0,
                antoineMinimumTemperature: 2142.0,
                densityLiquid: 17310,
                densitySolid: 19300,
                hardness: 202,
                isConductive: true,
                meltingPoint: 1337.15,
                youngsModulus: 78.5);

            /// <summary>
            /// the native form of the element Mercury
            /// </summary>
            public static readonly Chemical Mercury = new(
                new Formula(PeriodicTable.Instance[80]),
                "Mercury",
                antoineCoefficientA: 4.85767,
                antoineCoefficientB: 3007.129,
                antoineCoefficientC: -10.001,
                antoineMaximumTemperature: 749.99,
                antoineMinimumTemperature: 298.14,
                densityLiquid: 13534,
                densitySolid: 14184,
                isConductive: true,
                meltingPoint: 234.321);

            /// <summary>
            /// the native form of the element Thallium
            /// </summary>
            public static readonly Chemical Thallium = new(
                new Formula(PeriodicTable.Instance[81]),
                "Thallium",
                antoineMaximumTemperature: 1746,
                antoineMinimumTemperature: 1746,
                densityLiquid: 11220,
                densitySolid: 11850,
                hardness: 35.6,
                isConductive: true,
                meltingPoint: 577,
                youngsModulus: 7.9);

            /// <summary>
            /// the native form of the element Lead
            /// </summary>
            public static readonly Chemical Lead = new(
                new Formula(PeriodicTable.Instance[82]),
                "Lead",
                antoineMaximumTemperature: 2022,
                antoineMinimumTemperature: 2022,
                densityLiquid: 10660,
                densitySolid: 11340,
                hardness: 44,
                isConductive: true,
                meltingPoint: 600.61,
                youngsModulus: 16.1);

            /// <summary>
            /// the native form of the element Bismuth
            /// </summary>
            public static readonly Chemical Bismuth = new(
                new Formula(PeriodicTable.Instance[83]),
                "Bismuth",
                antoineMaximumTemperature: 1837,
                antoineMinimumTemperature: 1837,
                densityLiquid: 9780,
                densitySolid: 10050,
                hardness: 82.5,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 544.7,
                youngsModulus: 34);

            /// <summary>
            /// the native form of the element Polonium
            /// </summary>
            public static readonly Chemical Polonium = new(
                new Formula(PeriodicTable.Instance[84]),
                "Polonium",
                antoineMaximumTemperature: 1235,
                antoineMinimumTemperature: 1235,
                densitySolid: 9196,
                isConductive: true,
                meltingPoint: 527,
                youngsModulus: 26);

            /// <summary>
            /// the native form of the element Astatine
            /// </summary>
            public static readonly Chemical Astatine = new(
                new Formula((PeriodicTable.Instance[85], 2)),
                "Astatine",
                antoineMaximumTemperature: 610,
                antoineMinimumTemperature: 610,
                densitySolid: 6350,
                meltingPoint: 575);

            /// <summary>
            /// the native form of the element Radon
            /// </summary>
            public static readonly Chemical Radon = new(
                new Formula(PeriodicTable.Instance[86]),
                "Radon",
                antoineCoefficientA: 3.1908,
                antoineCoefficientB: 558.293,
                antoineCoefficientC: -36.803,
                antoineMaximumTemperature: 211.8,
                antoineMinimumTemperature: 129.0,
                densityLiquid: 4400,
                meltingPoint: 544.7);

            /// <summary>
            /// the native form of the element Francium
            /// </summary>
            public static readonly Chemical Francium = new(
                new Formula(PeriodicTable.Instance[87]),
                "Francium",
                antoineMaximumTemperature: 950,
                antoineMinimumTemperature: 950,
                densitySolid: 2900,
                isConductive: true,
                meltingPoint: 300);

            /// <summary>
            /// the native form of the element Radium
            /// </summary>
            public static readonly Chemical Radium = new(
                new Formula(PeriodicTable.Instance[88]),
                "Radium",
                antoineMaximumTemperature: 2010,
                antoineMinimumTemperature: 2010,
                densitySolid: 5500,
                isConductive: true,
                meltingPoint: 973,
                youngsModulus: 13.2);

            /// <summary>
            /// the native form of the element Actinium
            /// </summary>
            public static readonly Chemical Actinium = new(
                new Formula(PeriodicTable.Instance[89]),
                "Actinium",
                antoineMaximumTemperature: 3500,
                antoineMinimumTemperature: 3500,
                densitySolid: 10000,
                isConductive: true,
                meltingPoint: 1500,
                youngsModulus: 25);

            /// <summary>
            /// the native form of the element Thorium
            /// </summary>
            public static readonly Chemical Thorium = new(
                new Formula(PeriodicTable.Instance[90]),
                "Thorium",
                antoineMaximumTemperature: 5061,
                antoineMinimumTemperature: 5061,
                densitySolid: 11700,
                hardness: 537.5,
                isConductive: true,
                meltingPoint: 2023,
                youngsModulus: 78.3);

            /// <summary>
            /// the native form of the element Protactinium
            /// </summary>
            public static readonly Chemical Protactinium = new(
                new Formula(PeriodicTable.Instance[91]),
                "Protactinium",
                antoineMaximumTemperature: 4300,
                antoineMinimumTemperature: 4300,
                densitySolid: 15370,
                isConductive: true,
                meltingPoint: 1841,
                youngsModulus: 76);

            /// <summary>
            /// the native form of the element Uranium
            /// </summary>
            public static readonly Chemical Uranium = new(
                new Formula(PeriodicTable.Instance[92]),
                "Uranium",
                antoineMaximumTemperature: 4404,
                antoineMinimumTemperature: 4404,
                densityLiquid: 17300,
                densitySolid: 19100,
                hardness: 2425,
                isConductive: true,
                meltingPoint: 1405.3,
                youngsModulus: 177);

            /// <summary>
            /// the native form of the element Neptunium
            /// </summary>
            public static readonly Chemical Neptunium = new(
                new Formula(PeriodicTable.Instance[93]),
                "Neptunium",
                antoineCoefficientA: 4.15718,
                antoineCoefficientB: 19215.926,
                antoineCoefficientC: -114.171,
                antoineMaximumTemperature: 2073.99,
                antoineMinimumTemperature: 1617.99,
                densitySolid: 19380,
                isConductive: true,
                meltingPoint: 912,
                youngsModulus: 68);

            /// <summary>
            /// the native form of the element Plutonium
            /// </summary>
            public static readonly Chemical Plutonium = new(
                new Formula(PeriodicTable.Instance[94]),
                "Plutonium",
                antoineMaximumTemperature: 3505,
                antoineMinimumTemperature: 3505,
                densityLiquid: 16630,
                densitySolid: 19816,
                isConductive: true,
                meltingPoint: 912.5,
                youngsModulus: 87.5);

            /// <summary>
            /// the native form of the element Americium
            /// </summary>
            public static readonly Chemical Americium = new(
                new Formula(PeriodicTable.Instance[95]),
                "Americium",
                antoineMaximumTemperature: 2880,
                antoineMinimumTemperature: 2880,
                densitySolid: 12000,
                isConductive: true,
                meltingPoint: 1449);

            /// <summary>
            /// the native form of the element Curium
            /// </summary>
            public static readonly Chemical Curium = new(
                new Formula(PeriodicTable.Instance[96]),
                "Curium",
                antoineMaximumTemperature: 3383,
                antoineMinimumTemperature: 3383,
                densitySolid: 13510,
                isConductive: true,
                meltingPoint: 1613);

            /// <summary>
            /// the native form of the element Berkelium
            /// </summary>
            public static readonly Chemical Berkelium = new(
                new Formula(PeriodicTable.Instance[97]),
                "Berkelium",
                antoineMaximumTemperature: 2900,
                antoineMinimumTemperature: 2900,
                densitySolid: 13250,
                meltingPoint: 1259);

            /// <summary>
            /// the native form of the element Californium
            /// </summary>
            public static readonly Chemical Californium = new(
                new Formula(PeriodicTable.Instance[98]),
                "Californium",
                antoineMaximumTemperature: 2900,
                antoineMinimumTemperature: 2900,
                densitySolid: 13250,
                meltingPoint: 1259);

            /// <summary>
            /// the native form of the element Einsteinium
            /// </summary>
            public static readonly Chemical Einsteinium = new(
                new Formula(PeriodicTable.Instance[99]),
                "Einsteinium",
                antoineMaximumTemperature: 1269,
                antoineMinimumTemperature: 1269,
                densitySolid: 8840,
                meltingPoint: 1133);

            /// <summary>
            /// the native form of the element Fermium
            /// </summary>
            public static readonly Chemical Fermium = new(
                new Formula(PeriodicTable.Instance[100]),
                "Fermium",
                densitySolid: 9710,
                meltingPoint: 1800);

            /// <summary>
            /// the native form of the element Mendelevium
            /// </summary>
            public static readonly Chemical Mendelevium = new(
                new Formula(PeriodicTable.Instance[101]),
                "Mendelevium",
                densitySolid: 10370,
                meltingPoint: 1100);

            /// <summary>
            /// the native form of the element Nobelium
            /// </summary>
            public static readonly Chemical Nobelium = new(
                new Formula(PeriodicTable.Instance[102]),
                "Nobelium",
                densitySolid: 9940,
                meltingPoint: 1100);

            /// <summary>
            /// the native form of the element Lawrencium
            /// </summary>
            public static readonly Chemical Lawrencium = new(
                new Formula(PeriodicTable.Instance[103]),
                "Lawrencium",
                densitySolid: 16100,
                meltingPoint: 1900);

            /// <summary>
            /// the native form of the element Rutherfordium
            /// </summary>
            public static readonly Chemical Rutherfordium = new(
                new Formula(PeriodicTable.Instance[104]),
                "Rutherfordium",
                antoineMaximumTemperature: 5800,
                antoineMinimumTemperature: 5800,
                densitySolid: 23200,
                meltingPoint: 2400);

            /// <summary>
            /// the native form of the element Dubnium
            /// </summary>
            public static readonly Chemical Dubnium = new(
                new Formula(PeriodicTable.Instance[105]),
                "Dubnium",
                densitySolid: 29300);

            /// <summary>
            /// the native form of the element Seaborgium
            /// </summary>
            public static readonly Chemical Seaborgium = new(
                new Formula(PeriodicTable.Instance[106]),
                "Seaborgium",
                densitySolid: 35000);

            /// <summary>
            /// the native form of the element Bohrium
            /// </summary>
            public static readonly Chemical Bohrium = new(
                new Formula(PeriodicTable.Instance[107]),
                "Bohrium",
                densitySolid: 37100);

            /// <summary>
            /// the native form of the element Hassium
            /// </summary>
            public static readonly Chemical Hassium = new(
                new Formula(PeriodicTable.Instance[108]),
                "Hassium",
                densitySolid: 41000);

            /// <summary>
            /// the native form of the element Meitnerium
            /// </summary>
            public static readonly Chemical Meitnerium = new(
                new Formula(PeriodicTable.Instance[109]),
                "Meitnerium",
                densitySolid: 37400);

            /// <summary>
            /// the native form of the element Darmstadtium
            /// </summary>
            public static readonly Chemical Darmstadtium = new(
                new Formula(PeriodicTable.Instance[110]),
                "Darmstadtium",
                densitySolid: 34800);

            /// <summary>
            /// the native form of the element Roentgenium
            /// </summary>
            public static readonly Chemical Roentgenium = new(
                new Formula(PeriodicTable.Instance[111]),
                "Roentgenium",
                densitySolid: 28700);

            /// <summary>
            /// the native form of the element Copernicium
            /// </summary>
            public static readonly Chemical Copernicium = new(
                new Formula(PeriodicTable.Instance[112]),
                "Copernicium",
                antoineMaximumTemperature: 357,
                antoineMinimumTemperature: 357,
                densityLiquid: 23700);

            /// <summary>
            /// the native form of the element Flerovium
            /// </summary>
            public static readonly Chemical Nihonium = new(
                new Formula(PeriodicTable.Instance[113]),
                "Nihonium",
                antoineMaximumTemperature: 1430,
                antoineMinimumTemperature: 1430,
                densitySolid: 16000,
                meltingPoint: 700);

            /// <summary>
            /// the native form of the element Flerovium
            /// </summary>
            public static readonly Chemical Flerovium = new(
                new Formula(PeriodicTable.Instance[114]),
                "Flerovium",
                antoineMaximumTemperature: 210,
                antoineMinimumTemperature: 210,
                densityLiquid: 14000);

            /// <summary>
            /// the native form of the element Moscovium
            /// </summary>
            public static readonly Chemical Moscovium = new(
                new Formula(PeriodicTable.Instance[115]),
                "Moscovium",
                antoineMaximumTemperature: 1400,
                antoineMinimumTemperature: 1400,
                densitySolid: 13500,
                meltingPoint: 670);

            /// <summary>
            /// the native form of the element Livermorium
            /// </summary>
            public static readonly Chemical Livermorium = new(
                new Formula(PeriodicTable.Instance[116]),
                "Livermorium",
                antoineMaximumTemperature: 1085,
                antoineMinimumTemperature: 1085,
                densitySolid: 12900,
                meltingPoint: 708.5);

            /// <summary>
            /// the native form of the element Tennessine
            /// </summary>
            public static readonly Chemical Tennessine = new(
                new Formula(PeriodicTable.Instance[117]),
                "Tennessine",
                antoineMaximumTemperature: 883,
                antoineMinimumTemperature: 883,
                densitySolid: 7250,
                meltingPoint: 723);

            /// <summary>
            /// the native form of the element Oganesson
            /// </summary>
            public static readonly Chemical Oganesson = new(
                new Formula(PeriodicTable.Instance[118]),
                "Oganesson",
                antoineMaximumTemperature: 350,
                antoineMinimumTemperature: 350,
                densityLiquid: 5500);

            #endregion Elements

            #region Gems

            /// <summary>
            /// Beryl
            /// </summary>
            public static readonly Chemical Beryl = new(
                    Formula.Parse("Be3Al2Si6O18"),
                    "Beryl",
                    densityLiquid: 2715,
                    densitySolid: 2715,
                    hardness: 1500,
                    isGemstone: true,
                    meltingPoint: 2570,
                    youngsModulus: 211);

            /// <summary>
            /// Corundum
            /// </summary>
            public static readonly Chemical Corundum = new(
                    Formula.Parse("Al2O3"),
                    "Corundum",
                    densityLiquid: 4000,
                    densitySolid: 4000,
                    hardness: 2750,
                    isGemstone: true,
                    meltingPoint: 2323.15,
                    youngsModulus: 400);

            /// <summary>
            /// Topaz
            /// </summary>
            public static readonly Chemical Topaz = new(
                    Formula.Parse("Al2SiO4(FOH)2"),
                    "Topaz",
                    densityLiquid: 3550,
                    densitySolid: 3550,
                    hardness: 1648,
                    isGemstone: true,
                    meltingPoint: 688.45,
                    youngsModulus: 290);

            #endregion Gems

            #region Hydrocarbons

            /// <summary>
            /// Adamantane
            /// </summary>
            public static readonly Chemical Adamantane = new(
                Formula.Parse("C10H16"),
                "Adamantane",
                antoineMaximumTemperature: 543,
                antoineMinimumTemperature: 543,
                densitySolid: 1080,
                isFlammable: true,
                meltingPoint: 543);

            /// <summary>
            /// Anthracene
            /// </summary>
            public static readonly Chemical Anthracene = new(
                Formula.Parse("C14H10"),
                "Anthracene",
                antoineCoefficientA: 4.72997,
                antoineCoefficientB: 2759.53,
                antoineCoefficientC: -30.753,
                antoineMaximumTemperature: 613.8,
                antoineMinimumTemperature: 496.4,
                densityLiquid: 969,
                densitySolid: 1280,
                isFlammable: true,
                meltingPoint: 489);

            /// <summary>
            /// Benzene
            /// </summary>
            public static readonly Chemical Benzene = new(
                Formula.Parse("C6H6"),
                "Benzene",
                antoineCoefficientA: 4.72583,
                antoineCoefficientB: 1660.652,
                antoineCoefficientC: -1.461,
                antoineMaximumTemperature: 373.5,
                antoineMinimumTemperature: 333.4,
                densityLiquid: 876.5,
                isFlammable: true,
                meltingPoint: 278.68);

            /// <summary>
            /// Biphenyl
            /// </summary>
            public static readonly Chemical Biphenyl = new(
                Formula.Parse("C12H10"),
                "Biphenyl",
                antoineCoefficientA: 4.35685,
                antoineCoefficientB: 1987.623,
                antoineCoefficientC: -71.556,
                antoineMaximumTemperature: 544.3,
                antoineMinimumTemperature: 342.3,
                densitySolid: 1040,
                isFlammable: true,
                meltingPoint: 342.3);

            /// <summary>
            /// Butane
            /// </summary>
            public static readonly Chemical Butane = new(
                Formula.Parse("C4H10"),
                "Butane",
                antoineCoefficientA: 4.35576,
                antoineCoefficientB: 1175.581,
                antoineCoefficientC: -2.071,
                antoineMaximumTemperature: 425,
                antoineMinimumTemperature: 272.66,
                densityLiquid: 625,
                isFlammable: true,
                meltingPoint: 136);

            /// <summary>
            /// Carbon Dioxide
            /// </summary>
            public static readonly Chemical CarbonDioxide = new(
                Formula.Parse("CO2"),
                "Carbon Dioxide",
                antoineCoefficientA: 6.93556,
                antoineCoefficientB: 1347.786,
                antoineCoefficientC: -0.15,
                antoineMaximumTemperature: 203.3,
                antoineMinimumTemperature: 153.2,
                densityLiquid: 1101,
                densitySolid: 1562,
                greenhousePotential: 1,
                meltingPoint: 195.15);

            /// <summary>
            /// Carbon Monoxide
            /// </summary>
            public static readonly Chemical CarbonMonoxide = new(
                Formula.Parse("CO"),
                "Carbon Monoxide",
                antoineCoefficientA: 3.81912,
                antoineCoefficientB: 291.743,
                antoineCoefficientC: -5.151,
                antoineMaximumTemperature: 88.1,
                antoineMinimumTemperature: 68.2,
                densityLiquid: 789,
                densitySolid: 789,
                isFlammable: true,
                meltingPoint: 68.15);

            /// <summary>
            /// Cyclobutane
            /// </summary>
            public static readonly Chemical Cyclobutane = new(
                Formula.Parse("C4H8"),
                "Cyclobutane",
                antoineCoefficientA: 4.07143,
                antoineCoefficientB: 1038.009,
                antoineCoefficientC: -30.3342,
                antoineMaximumTemperature: 285.34,
                antoineMinimumTemperature: 213.22,
                isFlammable: true,
                meltingPoint: 182);

            /// <summary>
            /// Cyclodecane
            /// </summary>
            public static readonly Chemical Cyclodecane = new(
                Formula.Parse("C10H20"),
                "Cyclodecane",
                antoineMaximumTemperature: 474,
                antoineMinimumTemperature: 474,
                densityLiquid: 871,
                isFlammable: true,
                meltingPoint: 282.5);

            /// <summary>
            /// Cyclododecane
            /// </summary>
            public static readonly Chemical Cyclododecane = new(
                Formula.Parse("C12H24"),
                "Cyclododecane",
                antoineMaximumTemperature: 517,
                antoineMinimumTemperature: 517,
                densitySolid: 790,
                isFlammable: true,
                meltingPoint: 333.9);

            /// <summary>
            /// Cycloheptane
            /// </summary>
            public static readonly Chemical Cycloheptane = new(
                Formula.Parse("C7H14"),
                "Cycloheptane",
                antoineCoefficientA: 3.9771,
                antoineCoefficientB: 1330.402,
                antoineCoefficientC: -56.946,
                antoineMaximumTemperature: 432.17,
                antoineMinimumTemperature: 341.3,
                densityLiquid: 811,
                isFlammable: true,
                meltingPoint: 261);

            /// <summary>
            /// Cyclohexane
            /// </summary>
            public static readonly Chemical Cyclohexane = new(
                Formula.Parse("C6H12"),
                "Cyclohexane",
                antoineCoefficientA: 3.96988,
                antoineCoefficientB: 1203.526,
                antoineCoefficientC: -50.287,
                antoineMaximumTemperature: 354.73,
                antoineMinimumTemperature: 293.06,
                densityLiquid: 778.1,
                isFlammable: true,
                meltingPoint: 279.62);

            /// <summary>
            /// Cyclononane
            /// </summary>
            public static readonly Chemical Cyclononane = new(
                Formula.Parse("C9H18"),
                "Cyclononane",
                antoineMaximumTemperature: 448,
                antoineMinimumTemperature: 448,
                densityLiquid: 853.4,
                isFlammable: true,
                meltingPoint: 283.65);

            /// <summary>
            /// Cyclooctane
            /// </summary>
            public static readonly Chemical Cyclooctane = new(
                Formula.Parse("C8H16"),
                "Cyclooctane",
                antoineCoefficientA: 3.98805,
                antoineCoefficientB: 1438.687,
                antoineCoefficientC: -63.024,
                antoineMaximumTemperature: 467.6,
                antoineMinimumTemperature: 369.86,
                densityLiquid: 834,
                isFlammable: true,
                meltingPoint: 287.74);

            /// <summary>
            /// Cyclopentane
            /// </summary>
            public static readonly Chemical Cyclopentane = new(
                Formula.Parse("C5H10"),
                "Cyclopentane",
                antoineCoefficientA: 4.00288,
                antoineCoefficientB: 1119.208,
                antoineCoefficientC: -42.412,
                antoineMaximumTemperature: 323.18,
                antoineMinimumTemperature: 288.86,
                densityLiquid: 751,
                isFlammable: true,
                meltingPoint: 179.2);

            /// <summary>
            /// Cyclopropane
            /// </summary>
            public static readonly Chemical Cyclopropane = new(
                Formula.Parse("C3H6"),
                "Cyclopropane",
                antoineCoefficientA: 4.05015,
                antoineCoefficientB: 870.393,
                antoineCoefficientC: -25.063,
                antoineMaximumTemperature: 241.07,
                antoineMinimumTemperature: 183.12,
                isFlammable: true,
                meltingPoint: 145);

            /// <summary>
            /// Cyclotetradecane
            /// </summary>
            public static readonly Chemical Cyclotetradecane = new(
                Formula.Parse("C14H28"),
                "Cyclotetradecane",
                antoineMaximumTemperature: 554.05,
                antoineMinimumTemperature: 554.05,
                densitySolid: 800,
                isFlammable: true,
                meltingPoint: 327.15);

            /// <summary>
            /// Cyclotridecane
            /// </summary>
            public static readonly Chemical Cyclotridecane = new(
                Formula.Parse("C13H26"),
                "Cyclotridecane",
                antoineMaximumTemperature: 534.15,
                antoineMinimumTemperature: 534.15,
                densitySolid: 800,
                isFlammable: true,
                meltingPoint: 296.15);

            /// <summary>
            /// Cycloundecane
            /// </summary>
            public static readonly Chemical Cycloundecane = new(
                Formula.Parse("C11H22"),
                "Cycloundecane",
                antoineMaximumTemperature: 491.55,
                antoineMinimumTemperature: 491.55,
                densityLiquid: 800,
                isFlammable: true,
                meltingPoint: 266.15);

            /// <summary>
            /// Cumene
            /// </summary>
            public static readonly Chemical Cumene = new(
                Formula.Parse("C9H12"),
                "Cumene",
                antoineCoefficientA: 4.05419,
                antoineCoefficientB: 1455.811,
                antoineCoefficientC: -65.948,
                antoineMaximumTemperature: 426.52,
                antoineMinimumTemperature: 343.17,
                densityLiquid: 862,
                isFlammable: true,
                meltingPoint: 177);

            /// <summary>
            /// Decalin
            /// </summary>
            public static readonly Chemical Decalin = new(
                Formula.Parse("C10H18"),
                "Decalin",
                antoineCoefficientA: 3.99304,
                antoineCoefficientB: 1572.899,
                antoineCoefficientC: -65.947,
                antoineMaximumTemperature: 461.02,
                antoineMinimumTemperature: 365.51,
                densityLiquid: 896,
                isFlammable: true,
                meltingPoint: 242.7);

            /// <summary>
            /// Decane
            /// </summary>
            public static readonly Chemical Decane = new(
                Formula.Parse("C10H22"),
                "Decane",
                antoineCoefficientA: 4.07857,
                antoineCoefficientB: 1501.268,
                antoineCoefficientC: -78.67,
                antoineMaximumTemperature: 448.27,
                antoineMinimumTemperature: 367.63,
                densityLiquid: 730,
                isFlammable: true,
                meltingPoint: 243.3);

            /// <summary>
            /// Dodecane
            /// </summary>
            public static readonly Chemical Dodecane = new(
                Formula.Parse("C12H26"),
                "Dodecane",
                antoineMaximumTemperature: 489,
                antoineMinimumTemperature: 489,
                densityLiquid: 749.5,
                isFlammable: true,
                meltingPoint: 263.5);

            /// <summary>
            /// Durene
            /// </summary>
            public static readonly Chemical Durene = new(
                Formula.Parse("C10H14"),
                "Durene",
                antoineCoefficientA: 2.9204,
                antoineCoefficientB: 908.263,
                antoineCoefficientC: -160.447,
                antoineMaximumTemperature: 469.1,
                antoineMinimumTemperature: 318,
                densityLiquid: 868,
                isFlammable: true,
                meltingPoint: 352.3);

            /// <summary>
            /// Ethane
            /// </summary>
            public static readonly Chemical Ethane = new(
                Formula.Parse("C2H6"),
                "Ethane",
                antoineCoefficientA: 3.95405,
                antoineCoefficientB: 663.72,
                antoineCoefficientC: -16.469,
                antoineMaximumTemperature: 198.2,
                antoineMinimumTemperature: 130.4,
                densityLiquid: 554,
                densitySolid: 554,
                isFlammable: true,
                meltingPoint: 90.15);

            /// <summary>
            /// Ethylbenzene
            /// </summary>
            public static readonly Chemical Ethylbenzene = new(
                Formula.Parse("C8H10"),
                "Ethylbenzene",
                antoineCoefficientA: 4.07488,
                antoineCoefficientB: 1419.315,
                antoineCoefficientC: -60.539,
                antoineMaximumTemperature: 410.27,
                antoineMinimumTemperature: 329.74,
                densityLiquid: 866.5,
                isFlammable: true,
                meltingPoint: 178);

            /// <summary>
            /// Ethylene
            /// </summary>
            public static readonly Chemical Ethylene = new(
                Formula.Parse("C2H4"),
                "Ethylene",
                antoineCoefficientA: 3.87261,
                antoineCoefficientB: 584.146,
                antoineCoefficientC: -18.307,
                antoineMaximumTemperature: 188.57,
                antoineMinimumTemperature: 149.37,
                isFlammable: true,
                meltingPoint: 104);

            /// <summary>
            /// Heptane
            /// </summary>
            public static readonly Chemical Heptane = new(
                Formula.Parse("C7H16"),
                "Heptane",
                antoineCoefficientA: 4.02832,
                antoineCoefficientB: 1268.636,
                antoineCoefficientC: -56.199,
                antoineMaximumTemperature: 372.43,
                antoineMinimumTemperature: 299.07,
                densityLiquid: 679.5,
                isFlammable: true,
                meltingPoint: 182.601);

            /// <summary>
            /// Hexadecane
            /// </summary>
            public static readonly Chemical Hexadecane = new(
                Formula.Parse("C16H34"),
                "Hexadecane",
                antoineMaximumTemperature: 560,
                antoineMinimumTemperature: 560,
                densityLiquid: 770,
                isFlammable: true,
                meltingPoint: 291);

            /// <summary>
            /// Hexane
            /// </summary>
            public static readonly Chemical Hexane = new(
                Formula.Parse("C6H14"),
                "Hexane",
                antoineCoefficientA: 4.00266,
                antoineCoefficientB: 1171.53,
                antoineCoefficientC: -48.784,
                antoineMaximumTemperature: 342.69,
                antoineMinimumTemperature: 286.18,
                densityLiquid: 660.6,
                isFlammable: true,
                meltingPoint: 178);

            /// <summary>
            /// Hexene
            /// </summary>
            public static readonly Chemical Hexene = new(
                Formula.Parse("C6H12"),
                "Hexene",
                antoineCoefficientA: 3.99063,
                antoineCoefficientB: 1152.971,
                antoineCoefficientC: -47.301,
                antoineMaximumTemperature: 337.46,
                antoineMinimumTemperature: 289.04,
                densityLiquid: 673,
                isFlammable: true,
                meltingPoint: 133.3);

            /// <summary>
            /// Indane
            /// </summary>
            public static readonly Chemical Indane = new(
                Formula.Parse("C9H10"),
                "Indane",
                antoineMaximumTemperature: 449.6,
                antoineMinimumTemperature: 449.6,
                densityLiquid: 964.5,
                isFlammable: true,
                meltingPoint: 221.8);

            /// <summary>
            /// Indene
            /// </summary>
            public static readonly Chemical Indene = new(
                Formula.Parse("C9H8"),
                "Indene",
                antoineCoefficientA: 5.33514,
                antoineCoefficientB: 2511.452,
                antoineCoefficientC: 16.524,
                antoineMaximumTemperature: 454.8,
                antoineMinimumTemperature: 289.6,
                densityLiquid: 997,
                isFlammable: true,
                meltingPoint: 271.3);

            /// <summary>
            /// Methane
            /// </summary>
            public static readonly Chemical Methane = new(
                Formula.Parse("CH4"),
                "Methane",
                antoineCoefficientA: 3.7687,
                antoineCoefficientB: 395.744,
                antoineCoefficientC: -6.469,
                antoineMaximumTemperature: 120.6,
                antoineMinimumTemperature: 90.7,
                densityLiquid: 422.62,
                densitySolid: 422.62,
                greenhousePotential: 34,
                isFlammable: true,
                meltingPoint: 91.15);

            /// <summary>
            /// m-Xylene
            /// </summary>
            public static readonly Chemical MXylene = new(
                Formula.Parse("C8H10"),
                "m-Xylene",
                antoineCoefficientA: 4.13607,
                antoineCoefficientB: 1463.218,
                antoineCoefficientC: -57.991,
                antoineMaximumTemperature: 333,
                antoineMinimumTemperature: 273,
                densityLiquid: 860,
                isFlammable: true,
                meltingPoint: 225);

            /// <summary>
            /// Naphthalene
            /// </summary>
            public static readonly Chemical Naphthalene = new(
                Formula.Parse("C10H8"),
                "Naphthalene",
                antoineCoefficientA: 4.27117,
                antoineCoefficientB: 1831.571,
                antoineCoefficientC: -61.329,
                antoineMaximumTemperature: 452.30,
                antoineMinimumTemperature: 353.48,
                densityLiquid: 962.5,
                densitySolid: 1025.3,
                isFlammable: true,
                meltingPoint: 351.3);

            /// <summary>
            /// Nonane
            /// </summary>
            public static readonly Chemical Nonane = new(
                Formula.Parse("C9H20"),
                "Nonane",
                antoineCoefficientA: 4.06245,
                antoineCoefficientB: 1430.377,
                antoineCoefficientC: -71.355,
                antoineMaximumTemperature: 424.94,
                antoineMinimumTemperature: 343.49,
                densityLiquid: 718,
                isFlammable: true,
                meltingPoint: 219.5);

            /// <summary>
            /// Octane
            /// </summary>
            public static readonly Chemical Octane = new(
                Formula.Parse("C8H18"),
                "Octane",
                antoineCoefficientA: 4.04867,
                antoineCoefficientB: 1355.126,
                antoineCoefficientC: -63.633,
                antoineMaximumTemperature: 399.72,
                antoineMinimumTemperature: 326.08,
                densityLiquid: 703,
                isFlammable: true,
                meltingPoint: 216.3);

            /// <summary>
            /// o-Xylene
            /// </summary>
            public static readonly Chemical OXylene = new(
                Formula.Parse("C8H10"),
                "o-Xylene",
                antoineCoefficientA: 4.12928,
                antoineCoefficientB: 1478.244,
                antoineCoefficientC: -59.076,
                antoineMaximumTemperature: 418.52,
                antoineMinimumTemperature: 336.61,
                densityLiquid: 880,
                isFlammable: true,
                meltingPoint: 249);

            /// <summary>
            /// Pentadecane
            /// </summary>
            public static readonly Chemical Pentadecane = new(
                Formula.Parse("C15H32"),
                "Pentadecane",
                antoineMaximumTemperature: 543.15,
                antoineMinimumTemperature: 543.15,
                densityLiquid: 769,
                isFlammable: true,
                meltingPoint: 290);

            /// <summary>
            /// Pentane
            /// </summary>
            public static readonly Chemical Pentane = new(
                Formula.Parse("C5H12"),
                "Pentane",
                antoineCoefficientA: 3.9892,
                antoineCoefficientB: 1070.617,
                antoineCoefficientC: -40.454,
                antoineMaximumTemperature: 341.37,
                antoineMinimumTemperature: 268.8,
                densityLiquid: 620,
                isFlammable: true,
                meltingPoint: 143.4);

            /// <summary>
            /// Phenanthrene
            /// </summary>
            public static readonly Chemical Phenanthrene = new(
                Formula.Parse("C5H12"),
                "Phenanthrene",
                antoineCoefficientA: 4.6894,
                antoineCoefficientB: 2673.32,
                antoineCoefficientC: -40.7,
                antoineMaximumTemperature: 619.9,
                antoineMinimumTemperature: 476.8,
                densitySolid: 1180,
                isFlammable: true,
                meltingPoint: 374);

            /// <summary>
            /// Propane
            /// </summary>
            public static readonly Chemical Propane = new(
                Formula.Parse("C3H8"),
                "Propane",
                antoineCoefficientA: 3.98292,
                antoineCoefficientB: 819.296,
                antoineCoefficientC: -24.417,
                antoineMaximumTemperature: 320.7,
                antoineMinimumTemperature: 230.6,
                densityLiquid: 493,
                isFlammable: true,
                meltingPoint: 85.5);

            /// <summary>
            /// p-Xylene
            /// </summary>
            public static readonly Chemical PXylene = new(
                Formula.Parse("C8H10"),
                "p-Xylene",
                antoineCoefficientA: 4.11138,
                antoineCoefficientB: 1450.688,
                antoineCoefficientC: -58.16,
                antoineMaximumTemperature: 412.44,
                antoineMinimumTemperature: 331.44,
                densityLiquid: 861,
                isFlammable: true,
                meltingPoint: 286.3);

            /// <summary>
            /// Tetradecane
            /// </summary>
            public static readonly Chemical Tetradecane = new(
                Formula.Parse("C14H30"),
                "Tetradecane",
                antoineMaximumTemperature: 528,
                antoineMinimumTemperature: 528,
                densityLiquid: 762,
                isFlammable: true,
                meltingPoint: 278);

            /// <summary>
            /// Toluene
            /// </summary>
            public static readonly Chemical Toluene = new(
                Formula.Parse("C7H8"),
                "Toluene",
                antoineCoefficientA: 4.07827,
                antoineCoefficientB: 1343.943,
                antoineCoefficientC: -53.773,
                antoineMaximumTemperature: 384.66,
                antoineMinimumTemperature: 308.52,
                densityLiquid: 870,
                isFlammable: true,
                meltingPoint: 178);

            /// <summary>
            /// Tridecane
            /// </summary>
            public static readonly Chemical Tridecane = new(
                Formula.Parse("C13H28"),
                "Tridecane",
                antoineMaximumTemperature: 507,
                antoineMinimumTemperature: 507,
                densityLiquid: 756,
                isFlammable: true,
                meltingPoint: 268);

            /// <summary>
            /// Undecane
            /// </summary>
            public static readonly Chemical Undecane = new(
                Formula.Parse("C11H24"),
                "Undecane",
                antoineMaximumTemperature: 468,
                antoineMinimumTemperature: 468,
                densityLiquid: 740,
                isFlammable: true,
                meltingPoint: 247.4);

            #endregion Hydrocarbons

            #region Ions

            /// <summary>
            /// Alpha Particle (He²⁺)
            /// </summary>
            public static readonly Chemical AlphaParticle = new(
                Formula.Parse("He+2"),
                "Alpha Particle",
                antoineMaximumTemperature: 0,
                densityLiquid: 145,
                densitySolid: 145,
                isConductive: true,
                meltingPoint: 0.95,
                fixedPhase: PhaseType.Plasma);

            /// <summary>
            /// Bicarbonate
            /// </summary>
            public static readonly Chemical Bicarbonate = new(
                Formula.Parse("HCO3-1"),
                "Bicarbonate",
                antoineMaximumTemperature: 520.06,
                antoineMinimumTemperature: 520.06,
                densitySolid: 2200,
                meltingPoint: 336.12);

            /// <summary>
            /// C⁴⁺
            /// </summary>
            public static readonly Chemical C4Pos = new(
                Formula.Parse("C+4"),
                "C⁴⁺",
                antoineMaximumTemperature: 3915,
                antoineMinimumTemperature: 3915,
                densityLiquid: 1950,
                densitySolid: 1950,
                hardness: 5,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 3915);

            /// <summary>
            /// Ca²⁺
            /// </summary>
            public static readonly Chemical Ca2Pos = new(
                Formula.Parse("Ca+2"),
                "Ca²⁺",
                antoineCoefficientA: 2.78473,
                antoineCoefficientB: 3121.368,
                antoineCoefficientC: -594.591,
                antoineMaximumTemperature: 1712,
                antoineMinimumTemperature: 1254,
                densityLiquid: 1378,
                densitySolid: 1550,
                hardness: 293,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 1115);

            /// <summary>
            /// Cl⁻
            /// </summary>
            public static readonly Chemical Cl1Neg = new(
                Formula.Parse("Cl-1"),
                "Cl⁻",
                antoineCoefficientA: 3.0213,
                antoineCoefficientB: 530.591,
                antoineCoefficientC: -64.639,
                antoineMaximumTemperature: 239.4,
                antoineMinimumTemperature: 155,
                densityLiquid: 1562.5,
                densitySolid: 1562.5,
                isFlammable: true,
                meltingPoint: 171.6);

            /// <summary>
            /// Cr³⁺
            /// </summary>
            public static readonly Chemical Cr3Pos = new(
                Formula.Parse("Cr+3"),
                "Cr³⁺",
                antoineCoefficientA: 6.02371,
                antoineCoefficientB: 16064.989,
                antoineCoefficientC: -83.86,
                antoineMaximumTemperature: 2755,
                antoineMinimumTemperature: 1889,
                densityLiquid: 6300,
                densitySolid: 7190,
                hardness: 1060,
                isConductive: true,
                meltingPoint: 2180);

            /// <summary>
            /// Fe²⁺
            /// </summary>
            public static readonly Chemical Fe2Pos = new(
                Formula.Parse("Fe+2"),
                "Fe²⁺",
                antoineMaximumTemperature: 3134,
                antoineMinimumTemperature: 3134,
                densityLiquid: 6980,
                densitySolid: 7874,
                hardness: 608,
                isConductive: true,
                meltingPoint: 1811.15);

            /// <summary>
            /// Hydrogen Plasma (H⁺, i.e. a proton)
            /// </summary>
            public static readonly Chemical HydrogenPlasma = new(
                Formula.Parse("H+1"),
                "Hydrogen Plasma",
                antoineCoefficientA: 3.54314,
                antoineCoefficientB: 99.395,
                antoineCoefficientC: 7.726,
                antoineMaximumTemperature: 32.27,
                antoineMinimumTemperature: 21.01,
                densityLiquid: 70,
                densitySolid: 70,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 14.15,
                fixedPhase: PhaseType.Plasma);

            /// <summary>
            /// K⁺
            /// </summary>
            public static readonly Chemical K1Pos = new(
                Formula.Parse("K+1"),
                "K⁺",
                antoineCoefficientA: 4.45718,
                antoineCoefficientB: 4691.58,
                antoineCoefficientC: 24.195,
                antoineMaximumTemperature: 1033.0,
                antoineMinimumTemperature: 679.4,
                densityLiquid: 828,
                densitySolid: 862,
                hardness: 0.363,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 336.7);

            /// <summary>
            /// Mg²⁺
            /// </summary>
            public static readonly Chemical Mg2Pos = new(
                Formula.Parse("Mg+2"),
                "Mg²⁺",
                antoineMaximumTemperature: 1363,
                antoineMinimumTemperature: 1363,
                densityLiquid: 1584,
                densitySolid: 1738,
                hardness: 152,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 370.944);

            /// <summary>
            /// N⁵⁺
            /// </summary>
            public static readonly Chemical N5Pos = new(
                Formula.Parse("N+5"),
                "N⁵⁺",
                antoineCoefficientA: 3.61947,
                antoineCoefficientB: 255.68,
                antoineCoefficientC: -6.6,
                antoineMaximumTemperature: 83.7,
                antoineMinimumTemperature: 63.2,
                densityLiquid: 808,
                densitySolid: 808,
                meltingPoint: 63.15);

            /// <summary>
            /// Na⁺
            /// </summary>
            public static readonly Chemical Na1Pos = new(
                Formula.Parse("Na+1"),
                "Na⁺",
                antoineCoefficientA: 2.46077,
                antoineCoefficientB: 1873.728,
                antoineCoefficientC: -416.372,
                antoineMaximumTemperature: 1118.0,
                antoineMinimumTemperature: 924,
                densityLiquid: 927,
                densitySolid: 968,
                hardness: 0.69,
                isFlammable: true,
                isConductive: true,
                meltingPoint: 370.944);

            /// <summary>
            /// O²⁺
            /// </summary>
            public static readonly Chemical O2Pos = new(
                Formula.Parse("O+2"),
                "O²⁺",
                antoineCoefficientA: 3.81634,
                antoineCoefficientB: 319.01,
                antoineCoefficientC: -6.453,
                antoineMaximumTemperature: 97.2,
                antoineMinimumTemperature: 62.6,
                densityLiquid: 1141,
                densitySolid: 1141,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 54.36);

            /// <summary>
            /// S⁶⁺
            /// </summary>
            public static readonly Chemical S6Pos = new(
                Formula.Parse("S+6"),
                "S⁶⁺",
                antoineMaximumTemperature: 717.8,
                antoineMinimumTemperature: 717.8,
                densityLiquid: 1819,
                densitySolid: 1960,
                hardness: 16,
                isFlammable: true,
                meltingPoint: 388.36);

            /// <summary>
            /// Si⁴⁺
            /// </summary>
            public static readonly Chemical Si4Pos = new(
                Formula.Parse("Si+4"),
                "Si⁴⁺",
                antoineCoefficientA: 9.56436,
                antoineCoefficientB: 23308.848,
                antoineCoefficientC: -123.133,
                antoineMaximumTemperature: 2560.0,
                antoineMinimumTemperature: 1997.0,
                densityLiquid: 2570,
                densitySolid: 2329,
                hardness: 1224,
                meltingPoint: 1687);

            /// <summary>
            /// Sulfate
            /// </summary>
            public static readonly Chemical Sulfate = new(
                Formula.Parse("SO4-2"),
                "Sulfate",
                antoineMaximumTemperature: 897.04,
                antoineMinimumTemperature: 897.04,
                meltingPoint: 543.62);

            /// <summary>
            /// Ti⁴⁺
            /// </summary>
            public static readonly Chemical Ti4Pos = new(
                Formula.Parse("Ti+2"),
                "Ti⁴⁺",
                antoineMaximumTemperature: 3560,
                antoineMinimumTemperature: 3560,
                densityLiquid: 5500,
                densitySolid: 6000,
                hardness: 2125,
                isConductive: true,
                meltingPoint: 1941);

            #endregion Ions

            #region Minerals

            /// <summary>
            /// Acanthite
            /// </summary>
            public static readonly Chemical Acanthite = new(
                Formula.Parse("Ag2S"),
                "Acanthite",
                antoineMaximumTemperature: 1233.15,
                antoineMinimumTemperature: 1233.15,
                densitySolid: 7300,
                hardness: 226,
                meltingPoint: 1109);

            /// <summary>
            /// Albite
            /// </summary>
            public static readonly Chemical Albite = new(
                Formula.Parse("AlNaO8Si3"),
                "Albite",
                densitySolid: 2620,
                hardness: 1000,
                meltingPoint: 1110,
                youngsModulus: 73.4);

            /// <summary>
            /// Anorthite
            /// </summary>
            public static readonly Chemical Anorthite = new(
                Formula.Parse("Al2CaO8Si2"),
                "Anorthite",
                densitySolid: 2730,
                hardness: 817,
                meltingPoint: 1110,
                youngsModulus: 100);

            /// <summary>
            /// Boehmite
            /// </summary>
            public static readonly Chemical Boehmite = new(
                Formula.Parse("AlHO2"),
                "Boehmite",
                antoineMaximumTemperature: 3253.15,
                antoineMinimumTemperature: 3253.15,
                densitySolid: 3040,
                hardness: 1961,
                meltingPoint: 2323.15,
                youngsModulus: 140);

            /// <summary>
            /// Calcium Carbonate
            /// </summary>
            public static readonly Chemical CalciumCarbonate = new(
                Formula.Parse("CaCO3"),
                "Calcium Carbonate",
                antoineMaximumTemperature: 1098.15,
                antoineMinimumTemperature: 1098.15,
                densityLiquid: 2711,
                densitySolid: 2711,
                hardness: 250,
                meltingPoint: 1612,
                youngsModulus: 80.7);

            /// <summary>
            /// Cassiterite
            /// </summary>
            public static readonly Chemical Cassiterite = new(
                Formula.Parse("SnO2"),
                "Cassiterite",
                antoineMaximumTemperature: 2120,
                antoineMinimumTemperature: 2120,
                densitySolid: 6995,
                hardness: 13268,
                meltingPoint: 1900);

            /// <summary>
            /// Chalcopyrite
            /// </summary>
            public static readonly Chemical Chalcopyrite = new(
                Formula.Parse("CuFeS2"),
                "Chalcopyrite",
                densitySolid: 4200,
                hardness: 1834,
                meltingPoint: 1223.15);

            /// <summary>
            /// Chromite
            /// </summary>
            public static readonly Chemical Chromite = new(
                Formula.Parse("FeCr2O4"),
                "Chromite",
                densitySolid: 4650,
                hardness: 13405,
                meltingPoint: 2500);

            /// <summary>
            /// Cinnabar
            /// </summary>
            public static readonly Chemical Cinnabar = new(
                Formula.Parse("HgS"),
                "Cinnabar",
                densitySolid: 8176,
                hardness: 554,
                meltingPoint: 853);

            /// <summary>
            /// Diopside
            /// </summary>
            public static readonly Chemical Diopside = new(
                Formula.Parse("CaFeO6Si2"),
                "Diopside",
                densitySolid: 3400,
                hardness: 7700,
                meltingPoint: 1664.15);

            /// <summary>
            /// Enstatite
            /// </summary>
            public static readonly Chemical Enstatite = new(
                Formula.Parse("MgO3Si"),
                "Enstatite",
                densitySolid: 3200,
                hardness: 7700,
                meltingPoint: 1830.15,
                youngsModulus: 182);

            /// <summary>
            /// Fayalite
            /// </summary>
            public static readonly Chemical Fayalite = new(
                Formula.Parse("Fe2O4Si"),
                "Fayalite",
                densitySolid: 3200,
                hardness: 7000,
                meltingPoint: 1473.15);

            /// <summary>
            /// Ferrosilite
            /// </summary>
            public static readonly Chemical Ferrosilite = new(
                Formula.Parse("FeO3Si"),
                "Ferrosilite",
                densitySolid: 3200,
                hardness: 7700,
                meltingPoint: 1830.15);

            /// <summary>
            /// Forsterite
            /// </summary>
            public static readonly Chemical Forsterite = new(
                Formula.Parse("Mg2O4Si"),
                "Forsterite",
                densitySolid: 3270,
                hardness: 7110,
                meltingPoint: 2163.15);

            /// <summary>
            /// Galena
            /// </summary>
            public static readonly Chemical Galena = new(
                Formula.Parse("PbS"),
                "Galena",
                antoineMaximumTemperature: 1554,
                antoineMinimumTemperature: 1554,
                densitySolid: 7400,
                hardness: 897,
                meltingPoint: 1391);

            /// <summary>
            /// Gibbsite
            /// </summary>
            public static readonly Chemical Gibbsite = new(
                Formula.Parse("AlH3O3"),
                "Gibbsite",
                antoineMaximumTemperature: 2792,
                antoineMinimumTemperature: 2792,
                densitySolid: 2420,
                hardness: 1200,
                meltingPoint: 573);

            /// <summary>
            /// Goethite
            /// </summary>
            public static readonly Chemical Goethite = new(
                Formula.Parse("FeHO2"),
                "Goethite",
                densitySolid: 3550,
                hardness: 6541,
                meltingPoint: 409.15);

            /// <summary>
            /// Gypsum
            /// </summary>
            public static readonly Chemical Gypsum = new(
                Formula.Parse("CaH4O6S"),
                "Gypsum",
                densitySolid: 2317,
                hardness: 2000,
                meltingPoint: 398.15);

            /// <summary>
            /// Hematite
            /// </summary>
            public static readonly Chemical Hematite = new(
                Formula.Parse("Fe2O3"),
                "Hematite",
                densitySolid: 5300,
                hardness: 10296,
                meltingPoint: 1838.15);

            /// <summary>
            /// Hydroxyapatite
            /// </summary>
            public static readonly Chemical Hydroxyapatite = new(
                Formula.Parse("Ca10H2O26P6"),
                "Hydroxyapatite",
                densitySolid: 3180,
                hardness: 3430,
                meltingPoint: 1670,
                youngsModulus: 125);

            /// <summary>
            /// Ilmenite
            /// </summary>
            public static readonly Chemical Ilmenite = new(
                Formula.Parse("FeO3Ti"),
                "Ilmenite",
                densitySolid: 4745,
                hardness: 6198,
                meltingPoint: 1323.15);

            /// <summary>
            /// Kaolinite
            /// </summary>
            public static readonly Chemical Kaolinite = new(
                Formula.Parse("Al2H4O9Si2"),
                "Kaolinite",
                densitySolid: 2650,
                hardness: 42,
                meltingPoint: 2023.15,
                youngsModulus: 3.2);

            /// <summary>
            /// Magnetite
            /// </summary>
            public static readonly Chemical Magnetite = new(
                Formula.Parse("Fe3O4"),
                "Magnetite",
                antoineMaximumTemperature: 2896,
                antoineMinimumTemperature: 2896,
                densitySolid: 5150,
                hardness: 7223,
                isConductive: true,
                meltingPoint: 1870);

            /// <summary>
            /// Muscovite
            /// </summary>
            public static readonly Chemical Muscovite = new(
                Formula.Parse("Al3F2H2KO12Si3"),
                "Muscovite",
                densitySolid: 2820,
                hardness: 125,
                meltingPoint: 1548.15,
                youngsModulus: 48);

            /// <summary>
            /// Orthoclase
            /// </summary>
            public static readonly Chemical Orthoclase = new(
                Formula.Parse("AlKSi3O8"),
                "Orthoclase",
                densitySolid: 2560,
                hardness: 817,
                meltingPoint: 873.15,
                youngsModulus: 89);

            /// <summary>
            /// Potassium Nitrate
            /// </summary>
            public static readonly Chemical PotassiumNitrate = new(
                Formula.Parse("KNO3"),
                "Potassium Nitrate",
                densityLiquid: 2109,
                densitySolid: 2109,
                isFlammable: true, // Strictly speaking, it is an oxidizer, not flammable.
                meltingPoint: 607);

            /// <summary>
            /// Pyrite
            /// </summary>
            public static readonly Chemical Pyrite = new(
                Formula.Parse("FeS2"),
                "Pyrite",
                densitySolid: 4900,
                hardness: 1512.5,
                meltingPoint: 1455.65,
                youngsModulus: 291.5);

            /// <summary>
            /// Silicon Carbide
            /// </summary>
            public static readonly Chemical SiliconCarbide = new(
                Formula.Parse("CSi"),
                "Silicon Carbide",
                densitySolid: 3160,
                hardness: 2500,
                meltingPoint: 3100);

            /// <summary>
            /// Silicon Dioxide
            /// </summary>
            public static readonly Chemical SiliconDioxide = new(
                Formula.Parse("SiO2"),
                "Silicon Dioxide",
                antoineMinimumTemperature: 3220,
                antoineMaximumTemperature: 3220,
                densityLiquid: 2196,
                densitySolid: 2196,
                hardness: 10980,
                meltingPoint: 1923.15,
                youngsModulus: 95.5);

            /// <summary>
            /// Sodium Chloride
            /// </summary>
            public static readonly Chemical SodiumChloride = new(
                Formula.Parse("ClNa"),
                "Sodium Chloride",
                antoineCoefficientA: 5.07184,
                antoineCoefficientB: 8388.497,
                antoineCoefficientC: -82.638,
                antoineMaximumTemperature: 1738,
                antoineMinimumTemperature: 1138,
                densitySolid: 2170,
                hardness: 20,
                meltingPoint: 1073.8,
                youngsModulus: 39.98);

            /// <summary>
            /// Sperrylite
            /// </summary>
            public static readonly Chemical Sperrylite = new(
                Formula.Parse("PtAs2"),
                "Sperrylite",
                densitySolid: 10580,
                hardness: 1118.5,
                meltingPoint: 2041);

            #endregion Minerals

            #region Misc

            /// <summary>
            /// Aluminium Oxide
            /// </summary>
            public static readonly Chemical AluminiumOxide = new(
                Formula.Parse("Al2O3"),
                "Aluminium Oxide",
                antoineMaximumTemperature: 3250,
                antoineMinimumTemperature: 3250,
                densitySolid: 3987,
                meltingPoint: 2345);

            /// <summary>
            /// Calcium Hydroxide
            /// </summary>
            public static readonly Chemical CalciumHydroxide = new(
                Formula.Parse("CaH2O2"),
                "Calcium Hydroxide",
                antoineMaximumTemperature: 3123.15,
                antoineMinimumTemperature: 3123.15,
                densitySolid: 2211,
                meltingPoint: 853);

            /// <summary>
            /// Calcium Oxide
            /// </summary>
            public static readonly Chemical CalciumOxide = new(
                Formula.Parse("CaO"),
                "Calcium Oxide",
                antoineCoefficientA: 4.92531,
                antoineCoefficientB: 1432.526,
                antoineCoefficientC: -61.819,
                antoineMaximumTemperature: 3120,
                antoineMinimumTemperature: 3120,
                densitySolid: 3340,
                meltingPoint: 2886);

            /// <summary>
            /// Iron Sulfide
            /// </summary>
            public static readonly Chemical IronSulfide = new(
                Formula.Parse("FeS"),
                "Iron Sulfide",
                densitySolid: 4840,
                meltingPoint: 1467);

            /// <summary>
            /// Lead Oxide
            /// </summary>
            public static readonly Chemical LeadOxide = new(
                Formula.Parse("PbO"),
                "Lead Oxide",
                antoineMaximumTemperature: 1750,
                antoineMinimumTemperature: 1750,
                densitySolid: 9530,
                meltingPoint: 1161);

            /// <summary>
            /// Lead-206 Oxide
            /// </summary>
            public static readonly Chemical Lead206Oxide = new(
                Formula.Parse("{206}PbO"),
                "Lead-206 Oxide",
                antoineMaximumTemperature: 1750,
                antoineMinimumTemperature: 1750,
                densitySolid: 9530,
                meltingPoint: 1161);

            /// <summary>
            /// Phosphoric Acid
            /// </summary>
            public static readonly Chemical PhosphoricAcid = new(
                Formula.Parse("H3PO4"),
                "Phosphoric Acid",
                antoineMinimumTemperature: 431,
                antoineMaximumTemperature: 431,
                densityLiquid: 2030,
                densitySolid: 2030,
                meltingPoint: 315.45);

            /// <summary>
            /// Sodium Oxide
            /// </summary>
            public static readonly Chemical SodiumOxide = new(
                Formula.Parse("Na2O"),
                "Sodium Oxide",
                antoineMaximumTemperature: 2220,
                antoineMinimumTemperature: 2220,
                densitySolid: 2270,
                meltingPoint: 1405);

            /// <summary>
            /// Sulfuric Acid
            /// </summary>
            public static readonly Chemical SulfuricAcid = new(
                Formula.Parse("H2SO4"),
                "Sulfuric Acid",
                antoineMaximumTemperature: 610,
                antoineMinimumTemperature: 610,
                densityLiquid: 1840,
                densitySolid: 1840,
                meltingPoint: 283);

            /// <summary>
            /// Water
            /// </summary>
            public static readonly Chemical Water = new(
                Formula.Parse("H2O"),
                "Water",
                antoineCoefficientA: 4.6543,
                antoineCoefficientB: 1435.264,
                antoineCoefficientC: -64.848,
                antoineMaximumTemperature: 373.0,
                antoineMinimumTemperature: 255.9,
                densityLiquid: 997,
                densitySolid: 919,
                greenhousePotential: 1,
                hardness: 8,
                isConductive: true,
                meltingPoint: 273.15);

            /// <summary>
            /// Zinc Sulfide
            /// </summary>
            public static readonly Chemical ZincSulfide = new(
                Formula.Parse("ZnS"),
                "Zinc Sulfide",
                antoineMaximumTemperature: 2120,
                antoineMinimumTemperature: 2120,
                densitySolid: 4090,
                hardness: 2128,
                meltingPoint: 2120);

            /// <summary>
            /// Uranium Dioxide
            /// </summary>
            public static readonly Chemical UraniumDioxide = new(
                Formula.Parse("O2U"),
                "Uranium Dioxide",
                densitySolid: 10970,
                hardness: 2128,
                meltingPoint: 3138);

            /// <summary>
            /// Uranium-235 Dioxide
            /// </summary>
            public static readonly Chemical Uranium235Dioxide = new(
                Formula.Parse("O2{235}U"),
                "Uranium-235 Dioxide",
                densitySolid: 10970,
                hardness: 2128,
                meltingPoint: 3138);

            #endregion Misc

            #region Organics

            /// <summary>
            /// Benzothiophene
            /// </summary>
            public static readonly Chemical Benzothiophene = new(
                Formula.Parse("C8H6S"),
                "Benzothiophene",
                antoineMaximumTemperature: 494,
                antoineMinimumTemperature: 494,
                densitySolid: 1150,
                meltingPoint: 305);

            /// <summary>
            /// Cellulose
            /// </summary>
            public static readonly Chemical Cellulose = new(
                Formula.Parse("C4800H8000O4000"),
                "Cellulose",
                densitySolid: 1500,
                isFlammable: true,
                youngsModulus: 20);

            /// <summary>
            /// Chitin
            /// </summary>
            public static readonly Chemical Chitin = new(
                Formula.Parse("C8H13O5N"),
                "Chitin",
                densitySolid: 1370,
                hardness: 1000,
                youngsModulus: 6);

            /// <summary>
            /// Collagen
            /// </summary>
            public static readonly Chemical Collagen = new(
                Formula.Parse("C57H91N19O16"),
                "Collagen",
                densitySolid: 1350,
                youngsModulus: 2);

            /// <summary>
            /// Dibenzothiophene
            /// </summary>
            public static readonly Chemical Dibenzothiophene = new(
                Formula.Parse("C12H8S"),
                "Dibenzothiophene",
                antoineMaximumTemperature: 605.5,
                antoineMinimumTemperature: 605.5,
                densitySolid: 1252,
                meltingPoint: 371.5);

            /// <summary>
            /// Elastin
            /// </summary>
            public static readonly Chemical Elastin = new(
                Formula.Parse("C27H48N6O6"),
                "Elastin",
                antoineMaximumTemperature: 373.15,
                antoineMinimumTemperature: 373.15,
                youngsModulus: 0.001);

            /// <summary>
            /// Ethanol
            /// </summary>
            public static readonly Chemical Ethanol = new(
                Formula.Parse("C2H6O"),
                "Ethanol",
                antoineCoefficientA: 4.92531,
                antoineCoefficientB: 1432.526,
                antoineCoefficientC: -61.819,
                antoineMaximumTemperature: 513.91,
                antoineMinimumTemperature: 364.8,
                densityLiquid: 789.3,
                densitySolid: 789.3,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 158.8);

            /// <summary>
            /// Fructose
            /// </summary>
            public static readonly Chemical Fructose = new(
                Formula.Parse("C6H12O6"),
                "Fructose",
                densitySolid: 1694,
                isFlammable: true,
                meltingPoint: 376);

            /// <summary>
            /// Galactose
            /// </summary>
            public static readonly Chemical Galactose = new(
                Formula.Parse("C6H12O6"),
                "Galactose",
                densitySolid: 1500,
                meltingPoint: 442);

            /// <summary>
            /// Glucose
            /// </summary>
            public static readonly Chemical Glucose = new(
                Formula.Parse("C6H12O6"),
                "Glucose",
                densitySolid: 1540,
                isFlammable: true,
                meltingPoint: 423);

            /// <summary>
            /// Glycogen
            /// </summary>
            public static readonly Chemical Glycogen = new(
                Formula.Parse("C24H42O21"),
                "Glycogen",
                densitySolid: 1630,
                isFlammable: true,
                meltingPoint: 548.15);

            /// <summary>
            /// Isopropyl Alcohol
            /// </summary>
            public static readonly Chemical IsopropylAlcohol = new(
                Formula.Parse("C3H8O"),
                "Isopropyl Alcohol",
                antoineCoefficientA: 4.8610,
                antoineCoefficientB: 1357.427,
                antoineCoefficientC: -75.814,
                antoineMaximumTemperature: 362.41,
                antoineMinimumTemperature: 329.92,
                densityLiquid: 786,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 184);

            /// <summary>
            /// Lactose
            /// </summary>
            public static readonly Chemical Lactose = new(
                Formula.Parse("C12H22O11"),
                "Lactose",
                densitySolid: 1525,
                isFlammable: true,
                meltingPoint: 475.9);

            /// <summary>
            /// Lignin
            /// </summary>
            public static readonly Chemical Lignin = new(
                Formula.Parse("C1023H1122O363"),
                "Lignin",
                densitySolid: 1397,
                isFlammable: true,
                youngsModulus: 3.1);

            /// <summary>
            /// Maltose
            /// </summary>
            public static readonly Chemical Maltose = new(
                Formula.Parse("C12H22O11"),
                "Maltose",
                densitySolid: 1540,
                meltingPoint: 435.5);

            /// <summary>
            /// Methanol
            /// </summary>
            public static readonly Chemical Methanol = new(
                Formula.Parse("CH4O"),
                "Methanol",
                antoineCoefficientA: 5.20409,
                antoineCoefficientB: 1581.341,
                antoineCoefficientC: -33.50,
                antoineMaximumTemperature: 356.83,
                antoineMinimumTemperature: 288.1,
                densityLiquid: 792,
                isConductive: true,
                isFlammable: true,
                meltingPoint: 175.6);

            /// <summary>
            /// Potassium Carbonate
            /// </summary>
            public static readonly Chemical PotassiumCarbonate = new(
                Formula.Parse("K2CO3"),
                "Potassium Carbonate",
                densityLiquid: 2430,
                densitySolid: 2430,
                meltingPoint: 1164);

            /// <summary>
            /// Sodium Carbonate
            /// </summary>
            public static readonly Chemical SodiumCarbonate = new(
                Formula.Parse("Na2CO3"),
                "Sodium Carbonate",
                antoineMaximumTemperature: 1873.15,
                antoineMinimumTemperature: 1873.15,
                densitySolid: 2540,
                meltingPoint: 1124);

            /// <summary>
            /// Sucrose
            /// </summary>
            public static readonly Chemical Sucrose = new(
                Formula.Parse("C12H22O11"),
                "Sucrose",
                densitySolid: 1587,
                isFlammable: true);

            /// <summary>
            /// Triolein
            /// </summary>
            public static readonly Chemical Triolein = new(
                Formula.Parse("C57H104O6"),
                "Triolein",
                antoineMaximumTemperature: 827.4,
                antoineMinimumTemperature: 827.4,
                densityLiquid: 907.8,
                isFlammable: true,
                meltingPoint: 278);

            #endregion Organics

            #region Plastics

            /// <summary>
            /// Nylon
            /// </summary>
            public static readonly Chemical Nylon = new(
                Formula.Parse("C12H22N2O2"),
                "Nylon",
                densitySolid: 1314,
                meltingPoint: 537.15,
                youngsModulus: 3);

            /// <summary>
            /// Polycarbonate
            /// </summary>
            public static readonly Chemical Polycarbonate = new(
                Formula.Parse("C15H16O2"),
                "Polycarbonate",
                densitySolid: 1210,
                meltingPoint: 575.15,
                youngsModulus: 2.6);

            /// <summary>
            /// Polyester
            /// </summary>
            public static readonly Chemical Polyester = new(
                Formula.Parse("C10H8O4"),
                "Polyester",
                antoineMaximumTemperature: 623,
                antoineMinimumTemperature: 623,
                densitySolid: 1380,
                meltingPoint: 523);

            /// <summary>
            /// Polyethylene
            /// </summary>
            public static readonly Chemical Polyethylene = new(
                Formula.Parse("C2H4"),
                "Polyethylene",
                densitySolid: 920,
                meltingPoint: 398,
                youngsModulus: 1.5);

            /// <summary>
            /// Polypropylene
            /// </summary>
            public static readonly Chemical Polypropylene = new(
                Formula.Parse("C3H6"),
                "Polypropylene",
                densitySolid: 855,
                meltingPoint: 423.5,
                youngsModulus: 1.75);

            /// <summary>
            /// Polystyrene
            /// </summary>
            public static readonly Chemical Polystyrene = new(
                Formula.Parse("C8H8"),
                "Polystyrene",
                densitySolid: 1000,
                meltingPoint: 513,
                youngsModulus: 3.25);

            /// <summary>
            /// Polyvinyl Chloride
            /// </summary>
            public static readonly Chemical PolyvinylChloride = new(
                Formula.Parse("C2H3Cl"),
                "Polyvinyl Chloride",
                densitySolid: 1375,
                meltingPoint: 453.15,
                youngsModulus: 3.25);

            #endregion Plastics

            #endregion Chemicals

            #region HomogeneousSubstances

            #region Cosmic

            /// <summary>
            /// "fuzzball" which is the
            /// theoretical matter which comprises black holes.
            /// </summary>
            public static readonly HomogeneousSubstance Fuzzball = new(
                "Fuzzball",
                antoineMaximumTemperature: 0,
                antoineMinimumTemperature: 0,
                meltingPoint: 0);

            /// <summary>
            /// Neutron Degenerate Matter
            /// </summary>
            public static readonly HomogeneousSubstance NeutronDegenerateMatter = new(
                "Neutron Degenerate Matter",
                fixedPhase: PhaseType.NeutronDegenerateMatter,
                densitySpecial: 4e17);

            #endregion Cosmic

            #region Organics

            /// <summary>
            /// Blood
            /// </summary>
            public static readonly HomogeneousSubstance Blood = new(
                "Blood",
                antoineCoefficientA: 4.6543,
                antoineCoefficientB: 1435.264,
                antoineCoefficientC: -62.848,
                antoineMaximumTemperature: 373.0,
                antoineMinimumTemperature: 255.9,
                densityLiquid: 1060,
                isConductive: true,
                meltingPoint: 271.35,
                molarMass: 25.2);

            /// <summary>
            /// Flesh
            /// </summary>
            public static readonly HomogeneousSubstance Flesh = new(
                "Flesh",
                densitySolid: 976,
                isFlammable: true,
                youngsModulus: 0.22);

            /// <summary>
            /// Keratin
            /// </summary>
            public static readonly HomogeneousSubstance Keratin = new(
                "Keratin",
                densitySolid: 1170,
                hardness: 220,
                meltingPoint: 478.15,
                molarMass: 70,
                youngsModulus: 1.255);

            /// <summary>
            /// "protein" as a generic substance
            /// </summary>
            public static readonly HomogeneousSubstance Protein = new(
                "Protein",
                densitySolid: 1350,
                isFlammable: true,
                molarMass: 53,
                youngsModulus: 2);

            #endregion Organics

            #endregion HomogeneousSubstances

            #region Solutions

            #region Alloys

            /// <summary>
            /// Brass
            /// </summary>
            public static readonly Solution Brass = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Copper.GetHomogeneousReference(), 0.65m),
                    (Zinc.GetHomogeneousReference(), 0.35m),
                    },
                    "Brass",
                    densityLiquid: 8730,
                    densitySolid: 8730,
                    hardness: 1540,
                    meltingPoint: 1193.15,
                    youngsModulus: 113.5);

            /// <summary>
            /// Bronze
            /// </summary>
            public static readonly Solution Bronze = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Copper.GetHomogeneousReference(), 0.88m),
                    (WhiteTin.GetHomogeneousReference(), 0.12m),
                    },
                    "Bronze",
                    densityLiquid: 8565,
                    densitySolid: 8565,
                    hardness: 1569,
                    meltingPoint: 1223.15,
                    youngsModulus: 108);

            /// <summary>
            /// Carbon Steel
            /// </summary>
            public static readonly Solution CarbonSteel = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Iron.GetHomogeneousReference(), 0.9975m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.0025m),
                    },
                    "Carbon Steel",
                    densitySolid: 7850,
                    hardness: 1765,
                    meltingPoint: 1643.15,
                    youngsModulus: 180);

            /// <summary>
            /// Ferrochrome
            /// </summary>
            public static readonly Solution Ferrochrome = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Chromium.GetHomogeneousReference(), 0.6m),
                    (Iron.GetHomogeneousReference(), 0.4m),
                    },
                    "Ferrochrome");

            /// <summary>
            /// Adds an Iron-Nickel alloy similar to that hypothesized to form the Earth's core
            /// to the registry.
            /// </summary>
            public static readonly Solution IronNickelAlloy = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Iron.GetHomogeneousReference(), 0.945m),
                    (Nickel.GetHomogeneousReference(), 0.055m),
                    },
                    "IronNickelAlloy");

            /// <summary>
            /// Stainless Steel
            /// </summary>
            public static readonly Solution StainlessSteel = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Iron.GetHomogeneousReference(), 0.883m),
                    (Chromium.GetHomogeneousReference(), 0.105m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.012m),
                    },
                    "Stainless Steel",
                    densitySolid: 7850,
                    hardness: 1765,
                    meltingPoint: 1643.15,
                    youngsModulus: 180);

            #endregion Alloys

            #region Aqueous

            /// <summary>
            /// Seawater
            /// </summary>
            public static readonly Solution Seawater = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Water.GetHomogeneousReference(), 0.965m),
                    (Cl1Neg.GetHomogeneousReference(), 0.019354m),
                    (Na1Pos.GetHomogeneousReference(), 0.01077m),
                    (Sulfate.GetHomogeneousReference(), 0.002712m),
                    (Mg2Pos.GetHomogeneousReference(), 0.00129m),
                    (Ca2Pos.GetHomogeneousReference(), 0.0004121m),
                    (K1Pos.GetHomogeneousReference(), 0.000399m),
                    (Bicarbonate.GetHomogeneousReference(), 0.0001424m),
                    },
                    "Seawater",
                    antoineCoefficientA: 4.6543,
                    antoineCoefficientB: 1435.264,
                    antoineCoefficientC: -62.848,
                    antoineMaximumTemperature: 373.0,
                    antoineMinimumTemperature: 255.9,
                    densityLiquid: 1025,
                    densitySolid: 1025,
                    hardness: 8,
                    isConductive: true,
                    meltingPoint: 271.35);

            #endregion Aqueous

            #region Artificial

            /// <summary>
            /// Cement
            /// </summary>
            public static readonly Solution Cement = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (CalciumCarbonate.GetHomogeneousReference(), 0.85m),
                    (Gypsum.GetHomogeneousReference(), 0.05m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.026m),
                    (Kaolinite.GetHomogeneousReference(), 0.025m),
                    (Hematite.GetHomogeneousReference(), 0.025m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.008m),
                    (AluminiumOxide.GetHomogeneousReference(), 0.008m),
                    (CalciumOxide.GetHomogeneousReference(), 0.008m),
                    },
                    "Cement",
                    densitySolid: 2320,
                    youngsModulus: 27.5);

            /// <summary>
            /// Cotton Cloth
            /// </summary>
            public static readonly Solution CottonCloth = new(
                    Cellulose.GetHomogeneousReference(),
                    "Cotton Cloth",
                    densitySolid: 400,
                    isFlammable: true,
                    youngsModulus: 9.72);

            /// <summary>
            /// Leather
            /// </summary>
            public static readonly Solution Leather = new(
                    Cellulose.GetHomogeneousReference(),
                    "Leather",
                    densitySolid: 860,
                    isFlammable: true,
                    youngsModulus: 0.094);

            /// <summary>
            /// Paper
            /// </summary>
            public static readonly Solution Paper = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Cellulose.GetHomogeneousReference(), 0.825m),
                    (CalciumCarbonate.GetHomogeneousReference(), 0.14m),
                    (Kaolinite.GetHomogeneousReference(), 0.035m),
                    },
                    "Paper",
                    densitySolid: 1201,
                    isFlammable: true);

            /// <summary>
            /// Soda-Lime Glass
            /// </summary>
            public static readonly Solution SodaLimeGlass = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.75m),
                    (SodiumOxide.GetHomogeneousReference(), 0.13m),
                    (CalciumOxide.GetHomogeneousReference(), 0.105m),
                    (Corundum.GetHomogeneousReference(), 0.015m),
                    },
                    "Soda Lime Glass",
                    densityLiquid: 2520,
                    densitySolid: 2520,
                    hardness: 400,
                    meltingPoint: 1923.15,
                    fixedPhase: PhaseType.Glass,
                    youngsModulus: 70);

            /// <summary>
            /// Stoneware
            /// </summary>
            public static readonly Solution Stoneware = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Kaolinite.GetHomogeneousReference(), 0.74m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.17m),
                    (Orthoclase.GetHomogeneousReference(), 0.075m),
                    (Albite.GetHomogeneousReference(), 0.0375m),
                    (Anorthite.GetHomogeneousReference(), 0.0375m),
                    (Muscovite.GetHomogeneousReference(), 0.015m),
                    },
                    "Stoneware",
                    densityLiquid: 2403,
                    densitySolid: 2403,
                    hardness: 1200,
                    meltingPoint: 2300,
                    fixedPhase: PhaseType.Glass);

            #endregion Artificial

            #region Gems

            /// <summary>
            /// Emerald
            /// </summary>
            public static readonly Solution Emerald = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Beryl.GetHomogeneousReference(), 0.99m),
                    (Cr3Pos.GetHomogeneousReference(), 0.01m),
                    },
                    "Emerald",
                    isGemstone: true);

            /// <summary>
            /// Ruby
            /// </summary>
            public static readonly Solution Ruby = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Corundum.GetHomogeneousReference(), 0.99m),
                    (Cr3Pos.GetHomogeneousReference(), 0.01m),
                    },
                    "Ruby",
                    isGemstone: true,
                    youngsModulus: 345);

            /// <summary>
            /// Sapphire
            /// </summary>
            public static readonly Solution Sapphire = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Corundum.GetHomogeneousReference(), 0.999m),
                    (Fe2Pos.GetHomogeneousReference(), 0.0005m),
                    (Ti4Pos.GetHomogeneousReference(), 0.0005m),
                    },
                    "Sapphire",
                    isGemstone: true,
                    youngsModulus: 345);

            #endregion Gems

            #region Hydrocarbons

            /// <summary>
            /// Vitrinite
            /// </summary>
            public static readonly Solution Vitrinite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Naphthalene.GetHomogeneousReference(), 0.4m),
                    (Toluene.GetHomogeneousReference(), 0.3m),
                    (Biphenyl.GetHomogeneousReference(), 0.3m),
                    },
                    "Vitrinite",
                    densitySolid: 833,
                    hardness: 245);

            #endregion Hydrocarbons

            #region Minerals

            /// <summary>
            /// Olivine
            /// </summary>
            public static readonly Solution Olivine = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Forsterite.GetHomogeneousReference(), 0.7m),
                    (Fayalite.GetHomogeneousReference(), 0.3m),
                    },
                    "Olivine",
                    youngsModulus: 204);

            /// <summary>
            /// Cosmic Dust
            /// </summary>
            public static readonly Solution CosmicDust = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Olivine.GetHomogeneousReference(), 0.43518m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.43m),
                    (Water.GetHomogeneousReference(), 0.125m),
                    (SiliconCarbide.GetHomogeneousReference(), 0.0014m),
                    (Diamond.GetHomogeneousReference(), 0.0014m),
                    (CarbonMonoxide.GetHomogeneousReference(), 0.001m),
                    (CarbonDioxide.GetHomogeneousReference(), 0.001m),
                    (Naphthalene.GetHomogeneousReference(), 0.001m),
                    (Anthracene.GetHomogeneousReference(), 0.001m),
                    (Phenanthrene.GetHomogeneousReference(), 0.001m),
                    (Iron.GetHomogeneousReference(), 0.001m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.001m),
                    (Corundum.GetHomogeneousReference(), 0.00002m),
                    },
                    "Cosmic Dust",
                    densitySolid: 2000);

            /// <summary>
            /// Orthopyroxene
            /// </summary>
            public static readonly Solution Orthopyroxene = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Enstatite.GetHomogeneousReference(), 0.9m),
                    (Ferrosilite.GetHomogeneousReference(), 0.1m),
                    },
                    "Orthopyroxene");

            /// <summary>
            /// Plagioclase
            /// </summary>
            public static readonly Solution Plagioclase = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Albite.GetHomogeneousReference(), 0.5m),
                    (Anorthite.GetHomogeneousReference(), 0.5m),
                    },
                    "Plagioclase",
                    youngsModulus: 80);

            /// <summary>
            /// Sphalerite
            /// </summary>
            public static readonly Solution Sphalerite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (ZincSulfide.GetHomogeneousReference(), 0.95m),
                    (IronSulfide.GetHomogeneousReference(), 0.05m),
                    },
                    "Sphalerite",
                    densitySolid: 4050,
                    hardness: 2118,
                    meltingPoint: 1973.15);

            /// <summary>
            /// Uraninite
            /// </summary>
            public static readonly Solution Uraninite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (UraniumDioxide.GetHomogeneousReference(), 0.9368m),
                    (Lead206Oxide.GetHomogeneousReference(), 0.052m),
                    (Uranium235Dioxide.GetHomogeneousReference(), 0.0068m),
                    (Helium.GetHomogeneousReference(), 0.0037m),
                    (Radium.GetHomogeneousReference(), 0.0007m),
                    },
                    "Uraninite",
                    densitySolid: 10790);

            #endregion Minerals

            #region Organics

            /// <summary>
            /// Wood
            /// </summary>
            public static readonly Solution Wood = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Cellulose.GetHomogeneousReference(), 0.7m),
                    (Lignin.GetHomogeneousReference(), 0.3m),
                    },
                    "Wood",
                    densitySolid: 787,
                    hardness: 25.5,
                    isFlammable: true,
                    youngsModulus: 10);

            /// <summary>
            /// Wood
            /// </summary>
            public static readonly Solution WoodSmoke = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (CarbonDioxide.GetHomogeneousReference(), 0.451m),
                    (Water.GetHomogeneousReference(), 0.45m),
                    (CarbonMonoxide.GetHomogeneousReference(), 0.08m),
                    (Methane.GetHomogeneousReference(), 0.014m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.005m),
                    },
                    "Wood Smoke");

            #endregion Organics

            #endregion Solutions

            #region Mixtures

            #region Artificial

            /// <summary>
            /// Brick
            /// </summary>
            public static readonly Mixture Brick = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Kaolinite.GetHomogeneousReference(), 0.226m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.6m),
                    (Muscovite.GetHomogeneousReference(), 0.075m),
                    (CalciumHydroxide.GetHomogeneousReference(), 0.05m),
                    (Hematite.GetHomogeneousReference(), 0.049m),
                    },
                    "Brick");

            /// <summary>
            /// Concrete
            /// </summary>
            public static readonly Mixture Concrete = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.333m),
                    (CalciumCarbonate.GetHomogeneousReference(), 0.25m),
                    (Cement.GetHomogeneousReference(), 0.167m),
                    (Plagioclase.GetHomogeneousReference(), 0.1625m),
                    (Orthoclase.GetHomogeneousReference(), 0.0875m),
                    },
                    "Concrete");

            /// <summary>
            /// Earthenware
            /// </summary>
            public static readonly Mixture Earthenware = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.42m),
                    (Kaolinite.GetHomogeneousReference(), 0.385m),
                    (Orthoclase.GetHomogeneousReference(), 0.075m),
                    (Plagioclase.GetHomogeneousReference(), 0.075m),
                    (Muscovite.GetHomogeneousReference(), 0.045m)
                    },
                    "Earthenware");

            /// <summary>
            /// Reinforced Concrete
            /// </summary>
            public static readonly Mixture ReinforcedConcrete = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.33m),
                    (CalciumCarbonate.GetHomogeneousReference(), 0.2475m),
                    (Cement.GetHomogeneousReference(), 0.165m),
                    (Plagioclase.GetHomogeneousReference(), 0.16m),
                    (Orthoclase.GetHomogeneousReference(), 0.087m),
                    (CarbonSteel.GetHomogeneousReference(), 0.0105m),
                    },
                    "Reinforced Concrete");

            #endregion Artificial

            #region Cosmic

            /// <summary>
            /// Interplanetary Medium
            /// </summary>
            public static readonly Mixture InterplanetaryMedium = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (HydrogenPlasma.GetHomogeneousReference(), 0.7m),
                    (AlphaParticle.GetHomogeneousReference(), 0.28m),
                    (CosmicDust.GetHomogeneousReference(), 0.02m),
                    },
                    "Interplanetary Medium");

            /// <summary>
            /// Interstellar Medium
            /// </summary>
            public static readonly Mixture InterstellarMedium = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (HydrogenPlasma.GetHomogeneousReference(), 0.35m),
                    (Hydrogen.GetHomogeneousReference(), 0.35m),
                    (AlphaParticle.GetHomogeneousReference(), 0.14m),
                    (Helium.GetHomogeneousReference(), 0.14m),
                    (CosmicDust.GetHomogeneousReference(), 0.02m),
                    },
                    "Interstellar Medium");

            /// <summary>
            /// Intracluster Medium
            /// </summary>
            public static readonly Mixture IntraclusterMedium = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (HydrogenPlasma.GetHomogeneousReference(), 0.74m),
                    (AlphaParticle.GetHomogeneousReference(), 0.252m),
                    (O2Pos.GetHomogeneousReference(), 0.0039m),
                    (C4Pos.GetHomogeneousReference(), 0.00145m),
                    (Fe2Pos.GetHomogeneousReference(), 0.0008m),
                    (Neon.GetHomogeneousReference(), 0.0006m),
                    (N5Pos.GetHomogeneousReference(), 0.00045m),
                    (Si4Pos.GetHomogeneousReference(), 0.00035m),
                    (Mg2Pos.GetHomogeneousReference(), 0.00025m),
                    (S6Pos.GetHomogeneousReference(), 0.0002m),
                    },
                    "Intracluster Medium");

            /// <summary>
            /// Adds a mixture of ionized gases representative of many cosmic structures, such
            /// as HII regions and supernova remnants, to the registry.
            /// </summary>
            public static readonly Mixture IonizedCloud = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (HydrogenPlasma.GetHomogeneousReference(), 0.74m),
                    (AlphaParticle.GetHomogeneousReference(), 0.252m),
                    (O2Pos.GetHomogeneousReference(), 0.0039m),
                    (C4Pos.GetHomogeneousReference(), 0.00145m),
                    (Fe2Pos.GetHomogeneousReference(), 0.0008m),
                    (Neon.GetHomogeneousReference(), 0.0006m),
                    (N5Pos.GetHomogeneousReference(), 0.00045m),
                    (Si4Pos.GetHomogeneousReference(), 0.00035m),
                    (Mg2Pos.GetHomogeneousReference(), 0.00025m),
                    (S6Pos.GetHomogeneousReference(), 0.0002m),
                    },
                    "Ionized Cloud");

            /// <summary>
            /// Molecular Cloud
            /// </summary>
            public static readonly Mixture MolecularCloud = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Hydrogen.GetHomogeneousReference(), 0.74m),
                    (Helium.GetHomogeneousReference(), 0.252m),
                    (Oxygen.GetHomogeneousReference(), 0.0039m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.00145m),
                    (Iron.GetHomogeneousReference(), 0.0008m),
                    (Neon.GetHomogeneousReference(), 0.0006m),
                    (Nitrogen.GetHomogeneousReference(), 0.00045m),
                    (Silicon.GetHomogeneousReference(), 0.00035m),
                    (Magnesium.GetHomogeneousReference(), 0.00025m),
                    (Sulfur.GetHomogeneousReference(), 0.0002m),
                    },
                    "Molecular Cloud");

            /// <summary>
            /// Warm–Hot Intergalactic Medium
            /// </summary>
            public static readonly Mixture WarmHotIntergalacticMedium = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (HydrogenPlasma.GetHomogeneousReference(), 0.74m),
                    (AlphaParticle.GetHomogeneousReference(), 0.252m),
                    (O2Pos.GetHomogeneousReference(), 0.0039m),
                    (C4Pos.GetHomogeneousReference(), 0.00145m),
                    (Fe2Pos.GetHomogeneousReference(), 0.0008m),
                    (Neon.GetHomogeneousReference(), 0.0006m),
                    (N5Pos.GetHomogeneousReference(), 0.00045m),
                    (Si4Pos.GetHomogeneousReference(), 0.00035m),
                    (Mg2Pos.GetHomogeneousReference(), 0.00025m),
                    (S6Pos.GetHomogeneousReference(), 0.0002m),
                    },
                    "Warm-Hot Intergalactic Medium");

            #endregion Cosmic

            #region Hydrocarbons

            /// <summary>
            /// Anthracite
            /// </summary>
            public static readonly Mixture Anthracite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (AmorphousCarbon.GetHomogeneousReference(), 0.5m),
                    (Vitrinite.GetHomogeneousReference(), 0.345m),
                    (Water.GetHomogeneousReference(), 0.045m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.04m),
                    (Kaolinite.GetHomogeneousReference(), 0.03m),
                    (Pyrite.GetHomogeneousReference(), 0.01m),
                    (Methane.GetHomogeneousReference(), 0.0053m),
                    (Ethane.GetHomogeneousReference(), 0.0052m),
                    (CarbonMonoxide.GetHomogeneousReference(), 0.005m),
                    (HydrogenSulfide.GetHomogeneousReference(), 0.005m),
                    (CarbonDioxide.GetHomogeneousReference(), 0.0045m),
                    (Hydrogen.GetHomogeneousReference(), 0.003m),
                    (Ammonia.GetHomogeneousReference(), 0.002m)
                    },
                    "Anthracite",
                    densitySolid: 1350);

            /// <summary>
            /// Bituminous Coal
            /// </summary>
            public static readonly Mixture BituminousCoal = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Vitrinite.GetHomogeneousReference(), 0.6m),
                    (AmorphousCarbon.GetHomogeneousReference(), 0.1m),
                    (Water.GetHomogeneousReference(), 0.085m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.04m),
                    (Kaolinite.GetHomogeneousReference(), 0.03m),
                    (Pyrite.GetHomogeneousReference(), 0.025m),
                    (Methane.GetHomogeneousReference(), 0.0222m),
                    (Ethane.GetHomogeneousReference(), 0.022m),
                    (CarbonMonoxide.GetHomogeneousReference(), 0.021m),
                    (CarbonDioxide.GetHomogeneousReference(), 0.02m),
                    (Hydrogen.GetHomogeneousReference(), 0.013m),
                    (Ammonia.GetHomogeneousReference(), 0.01m),
                    (HydrogenSulfide.GetHomogeneousReference(), 0.01m),
                    },
                    "Bituminous Coal",
                    densityLiquid: 1346,
                    densitySolid: 833);

            /// <summary>
            /// Diesel
            /// </summary>
            public static readonly Mixture Diesel = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Nonane.GetHomogeneousReference(), 0.045m),
                    (Decane.GetHomogeneousReference(), 0.05m),
                    (Undecane.GetHomogeneousReference(), 0.065m),
                    (Dodecane.GetHomogeneousReference(), 0.07m),
                    (Tridecane.GetHomogeneousReference(), 0.065m),
                    (Tetradecane.GetHomogeneousReference(), 0.055m),
                    (Pentadecane.GetHomogeneousReference(), 0.05m),
                    (Hexadecane.GetHomogeneousReference(), 0.045m),
                    (Cyclononane.GetHomogeneousReference(), 0.04m),
                    (Cyclodecane.GetHomogeneousReference(), 0.045m),
                    (Cycloundecane.GetHomogeneousReference(), 0.055m),
                    (Cyclododecane.GetHomogeneousReference(), 0.065m),
                    (Cyclotridecane.GetHomogeneousReference(), 0.055m),
                    (Cyclotetradecane.GetHomogeneousReference(), 0.045m),
                    (Hexene.GetHomogeneousReference(), 0.018m),
                    (Benzene.GetHomogeneousReference(), 0.032m),
                    (Toluene.GetHomogeneousReference(), 0.032m),
                    (MXylene.GetHomogeneousReference(), 0.022m),
                    (OXylene.GetHomogeneousReference(), 0.022m),
                    (PXylene.GetHomogeneousReference(), 0.022m),
                    (Ethylbenzene.GetHomogeneousReference(), 0.014m),
                    (Cumene.GetHomogeneousReference(), 0.032m),
                    (Durene.GetHomogeneousReference(), 0.032m),
                    (Indane.GetHomogeneousReference(), 0.004m),
                    (Indene.GetHomogeneousReference(), 0.004m),
                    (Naphthalene.GetHomogeneousReference(), 0.004m),
                    (Phenanthrene.GetHomogeneousReference(), 0.004m),
                    (Benzothiophene.GetHomogeneousReference(), 0.004m),
                    (Dibenzothiophene.GetHomogeneousReference(), 0.004m),
                    },
                    "Diesel",
                    densityLiquid: 832);

            /// <summary>
            /// Gasoline
            /// </summary>
            public static readonly Mixture Gasoline = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Butane.GetHomogeneousReference(), 0.022m),
                    (Pentane.GetHomogeneousReference(), 0.181m),
                    (Hexane.GetHomogeneousReference(), 0.196m),
                    (Heptane.GetHomogeneousReference(), 0.031m),
                    (Octane.GetHomogeneousReference(), 0.018m),
                    (Nonane.GetHomogeneousReference(), 0.028m),
                    (Decane.GetHomogeneousReference(), 0.0047m),
                    (Undecane.GetHomogeneousReference(), 0.0047m),
                    (Dodecane.GetHomogeneousReference(), 0.0046m),
                    (Tridecane.GetHomogeneousReference(), 0.004m),
                    (Cyclohexane.GetHomogeneousReference(), 0.03m),
                    (Cycloheptane.GetHomogeneousReference(), 0.014m),
                    (Cyclooctane.GetHomogeneousReference(), 0.006m),
                    (Hexene.GetHomogeneousReference(), 0.018m),
                    (Benzene.GetHomogeneousReference(), 0.032m),
                    (Toluene.GetHomogeneousReference(), 0.048m),
                    (MXylene.GetHomogeneousReference(), 0.022m),
                    (OXylene.GetHomogeneousReference(), 0.022m),
                    (PXylene.GetHomogeneousReference(), 0.022m),
                    (Ethylbenzene.GetHomogeneousReference(), 0.014m),
                    (Cumene.GetHomogeneousReference(), 0.042m),
                    (Durene.GetHomogeneousReference(), 0.076m),
                    (Indane.GetHomogeneousReference(), 0.0045m),
                    (Indene.GetHomogeneousReference(), 0.0045m),
                    (Naphthalene.GetHomogeneousReference(), 0.0045m),
                    (Phenanthrene.GetHomogeneousReference(), 0.0045m),
                    (Benzothiophene.GetHomogeneousReference(), 0.0045m),
                    (Dibenzothiophene.GetHomogeneousReference(), 0.0045m),
                    (Ethanol.GetHomogeneousReference(), 0.073m),
                    (Methanol.GetHomogeneousReference(), 0.02m),
                    (PhosphoricAcid.GetHomogeneousReference(), 0.02m),
                    (IsopropylAlcohol.GetHomogeneousReference(), 0.02m),
                    },
                    "Gasoline",
                    densityLiquid: 708);

            /// <summary>
            /// Kerosine
            /// </summary>
            public static readonly Mixture Kerosine = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Hexane.GetHomogeneousReference(), 0.04m),
                    (Heptane.GetHomogeneousReference(), 0.043m),
                    (Octane.GetHomogeneousReference(), 0.048m),
                    (Nonane.GetHomogeneousReference(), 0.055m),
                    (Decane.GetHomogeneousReference(), 0.06m),
                    (Undecane.GetHomogeneousReference(), 0.058m),
                    (Dodecane.GetHomogeneousReference(), 0.056m),
                    (Tridecane.GetHomogeneousReference(), 0.05m),
                    (Tetradecane.GetHomogeneousReference(), 0.045m),
                    (Pentadecane.GetHomogeneousReference(), 0.04m),
                    (Hexadecane.GetHomogeneousReference(), 0.03m),
                    (Cyclohexane.GetHomogeneousReference(), 0.02m),
                    (Cycloheptane.GetHomogeneousReference(), 0.022m),
                    (Cyclooctane.GetHomogeneousReference(), 0.025m),
                    (Cyclononane.GetHomogeneousReference(), 0.026m),
                    (Cyclodecane.GetHomogeneousReference(), 0.027m),
                    (Cycloundecane.GetHomogeneousReference(), 0.026m),
                    (Cyclododecane.GetHomogeneousReference(), 0.025m),
                    (Cyclotridecane.GetHomogeneousReference(), 0.022m),
                    (Cyclotetradecane.GetHomogeneousReference(), 0.02m),
                    (Benzene.GetHomogeneousReference(), 0.032m),
                    (Toluene.GetHomogeneousReference(), 0.032m),
                    (MXylene.GetHomogeneousReference(), 0.015m),
                    (OXylene.GetHomogeneousReference(), 0.015m),
                    (PXylene.GetHomogeneousReference(), 0.015m),
                    (Ethylbenzene.GetHomogeneousReference(), 0.023m),
                    (Cumene.GetHomogeneousReference(), 0.015m),
                    (Durene.GetHomogeneousReference(), 0.015m),
                    (Indane.GetHomogeneousReference(), 0.033m),
                    (Indene.GetHomogeneousReference(), 0.009m),
                    (Naphthalene.GetHomogeneousReference(), 0.028m),
                    (Benzothiophene.GetHomogeneousReference(), 0.015m),
                    (Dibenzothiophene.GetHomogeneousReference(), 0.015m),
                    },
                    "Kerosine",
                    densityLiquid: 810);

            /// <summary>
            /// Natural Gas
            /// </summary>
            public static readonly Mixture NaturalGas = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Methane.GetHomogeneousReference(), 0.8819m),
                    (Ethane.GetHomogeneousReference(), 0.0517m),
                    (Propane.GetHomogeneousReference(), 0.0187m),
                    (Butane.GetHomogeneousReference(), 0.0077m),
                    (Pentane.GetHomogeneousReference(), 0.0025m),
                    (Hexane.GetHomogeneousReference(), 0.0007m),
                    (Helium.GetHomogeneousReference(), 0.001m),
                    (Nitrogen.GetHomogeneousReference(), 0.0354m),
                    (CarbonDioxide.GetHomogeneousReference(), 0.0004m),
                    },
                    "Natural Gas");

            /// <summary>
            /// Petroleum
            /// </summary>
            public static readonly Mixture Petroleum = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Pentane.GetHomogeneousReference(), 0.021m),
                    (Hexane.GetHomogeneousReference(), 0.021m),
                    (Heptane.GetHomogeneousReference(), 0.021m),
                    (Octane.GetHomogeneousReference(), 0.021m),
                    (Nonane.GetHomogeneousReference(), 0.021m),
                    (Decane.GetHomogeneousReference(), 0.021m),
                    (Undecane.GetHomogeneousReference(), 0.028m),
                    (Dodecane.GetHomogeneousReference(), 0.026m),
                    (Tridecane.GetHomogeneousReference(), 0.03m),
                    (Tetradecane.GetHomogeneousReference(), 0.03m),
                    (Pentadecane.GetHomogeneousReference(), 0.03m),
                    (Hexadecane.GetHomogeneousReference(), 0.03m),
                    (Cyclopentane.GetHomogeneousReference(), 0.029m),
                    (Cyclohexane.GetHomogeneousReference(), 0.029m),
                    (Cycloheptane.GetHomogeneousReference(), 0.029m),
                    (Cyclooctane.GetHomogeneousReference(), 0.029m),
                    (Cyclononane.GetHomogeneousReference(), 0.029m),
                    (Cyclodecane.GetHomogeneousReference(), 0.029m),
                    (Cycloundecane.GetHomogeneousReference(), 0.04m),
                    (Cyclododecane.GetHomogeneousReference(), 0.04m),
                    (Cyclotridecane.GetHomogeneousReference(), 0.04m),
                    (Cyclotetradecane.GetHomogeneousReference(), 0.04m),
                    (Decalin.GetHomogeneousReference(), 0.072m),
                    (Adamantane.GetHomogeneousReference(), 0.052m),
                    (Benzene.GetHomogeneousReference(), 0.06m),
                    (Indane.GetHomogeneousReference(), 0.02m),
                    (Indene.GetHomogeneousReference(), 0.02m),
                    (Naphthalene.GetHomogeneousReference(), 0.046m),
                    (Phenanthrene.GetHomogeneousReference(), 0.02m),
                    (Benzothiophene.GetHomogeneousReference(), 0.03m),
                    (Dibenzothiophene.GetHomogeneousReference(), 0.021m),
                    (Toluene.GetHomogeneousReference(), 0.004m),
                    (Ethylbenzene.GetHomogeneousReference(), 0.006m),
                    (MXylene.GetHomogeneousReference(), 0.005m),
                    (OXylene.GetHomogeneousReference(), 0.005m),
                    (PXylene.GetHomogeneousReference(), 0.005m),
                    },
                    "Petroleum",
                    densityLiquid: 800);

            #endregion Hydrocarbons

            #region Minerals

            /// <summary>
            /// Ball Clay
            /// </summary>
            public static readonly Mixture BallClay = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Kaolinite.GetHomogeneousReference(), 0.53m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.295m),
                    (Muscovite.GetHomogeneousReference(), 0.175m),
                    },
                    "Ball Clay");

            /// <summary>
            /// Basalt
            /// </summary>
            public static readonly Mixture Basalt = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Plagioclase.GetHomogeneousReference(), 0.65m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.35m),
                    },
                    "Basalt",
                    densitySolid: 3000);

            /// <summary>
            /// Bauxite
            /// </summary>
            public static readonly Mixture Bauxite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Gibbsite.GetHomogeneousReference(), 0.287m),
                    (Boehmite.GetHomogeneousReference(), 0.287m),
                    (Kaolinite.GetHomogeneousReference(), 0.116m),
                    (Goethite.GetHomogeneousReference(), 0.09m),
                    (Hematite.GetHomogeneousReference(), 0.09m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.085m),
                    (Ilmenite.GetHomogeneousReference(), 0.045m),
                    },
                    "Bauxite",
                    densitySolid: 2700);

            /// <summary>
            /// Granite
            /// </summary>
            public static readonly Mixture Granite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.6m),
                    (Plagioclase.GetHomogeneousReference(), 0.26m),
                    (Orthoclase.GetHomogeneousReference(), 0.14m),
                    },
                    "Granite",
                    densitySolid: 2700);

            /// <summary>
            /// Loam
            /// </summary>
            public static readonly Mixture Loam = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.5m),
                    (Kaolinite.GetHomogeneousReference(), 0.333m),
                    (Orthoclase.GetHomogeneousReference(), 0.056m),
                    (Albite.GetHomogeneousReference(), 0.056m),
                    (Anorthite.GetHomogeneousReference(), 0.055m),
                    },
                    "Loam",
                    densitySolid: 1250);

            /// <summary>
            /// Peridotite
            /// </summary>
            public static readonly Mixture Peridotite = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Olivine.GetHomogeneousReference(), 0.7m),
                    (Orthopyroxene.GetHomogeneousReference(), 0.18m),
                    (Diopside.GetHomogeneousReference(), 0.12m),
                    },
                    "Peridotite",
                    densitySolid: 3130);

            /// <summary>
            /// Sandstone
            /// </summary>
            public static readonly Mixture Sandstone = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.9m),
                    (Plagioclase.GetHomogeneousReference(), 0.05m),
                    (Orthoclase.GetHomogeneousReference(), 0.05m),
                    },
                    "Sandstone",
                    densitySolid: 2300);

            /// <summary>
            /// Silt
            /// </summary>
            public static readonly Mixture Silt = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (SiliconDioxide.GetHomogeneousReference(), 0.5m),
                    (Orthoclase.GetHomogeneousReference(), 0.167m),
                    (Albite.GetHomogeneousReference(), 0.167m),
                    (Anorthite.GetHomogeneousReference(), 0.166m),
                    },
                    "Silt");

            #endregion Minerals

            #region Misc

            /// <summary>
            /// Fly Ash
            /// </summary>
            public static readonly Mixture FlyAsh = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (AmorphousCarbon.GetHomogeneousReference(), 0.59m),
                    (SiliconDioxide.GetHomogeneousReference(), 0.3m),
                    (AluminiumOxide.GetHomogeneousReference(), 0.1m),
                    (CalciumOxide.GetHomogeneousReference(), 0.01m),
                    },
                    "Fly Ash",
                    densitySolid: 827);

            #endregion Misc

            #region Organics

            /// <summary>
            /// Adipose Tissue
            /// </summary>
            public static readonly Mixture AdiposeTissue = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Triolein.GetHomogeneousReference(), 0.87m),
                    (Water.GetHomogeneousReference(), 0.08m),
                    (Protein.GetHomogeneousReference(), 0.05m),
                    },
                    "Adipose Tissue",
                    densityLiquid: 900,
                    densitySolid: 900);

            /// <summary>
            /// Bone
            /// </summary>
            public static readonly Mixture Bone = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Hydroxyapatite.GetHomogeneousReference(), 0.7m),
                    (Collagen.GetHomogeneousReference(), 0.2775m),
                    (Water.GetHomogeneousReference(), 0.0225m),
                    },
                    "Bone",
                    densitySolid: 1050);

            /// <summary>
            /// Epithelial Tissue
            /// </summary>
            public static readonly Mixture EpithelialTissue = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Water.GetHomogeneousReference(), 0.7m),
                    (Keratin.GetHomogeneousReference(), 0.23m),
                    (Triolein.GetHomogeneousReference(), 0.02m),
                    (Protein.GetHomogeneousReference(), 0.02m),
                    },
                    "Epithelial Tissue",
                    densityLiquid: 1109,
                    densitySolid: 1109);

            /// <summary>
            /// Muscle Tissue
            /// </summary>
            public static readonly Mixture MuscleTissue = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Water.GetHomogeneousReference(), 0.75m),
                    (Protein.GetHomogeneousReference(), 0.2m),
                    (Triolein.GetHomogeneousReference(), 0.04m),
                    (Glycogen.GetHomogeneousReference(), 0.01m),
                    },
                    "Muscle Tissue",
                    densityLiquid: 1090,
                    densitySolid: 1090);

            /// <summary>
            /// Nervous Tissue
            /// </summary>
            public static readonly Mixture NervousTissue = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Water.GetHomogeneousReference(), 0.8m),
                    (Triolein.GetHomogeneousReference(), 0.104m),
                    (Protein.GetHomogeneousReference(), 0.078m),
                    (Glycogen.GetHomogeneousReference(), 0.018m),
                    },
                    "Nervous Tissue",
                    densityLiquid: 1075,
                    densitySolid: 1075);

            /// <summary>
            /// Tooth
            /// </summary>
            public static readonly Mixture Tooth = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (Hydroxyapatite.GetHomogeneousReference(), 0.792m),
                    (Collagen.GetHomogeneousReference(), 0.126m),
                    (Water.GetHomogeneousReference(), 0.082m),
                    },
                    "Tooth",
                    densityLiquid: 2180,
                    densitySolid: 2180);

            /// <summary>
            /// Wood Ash
            /// </summary>
            public static readonly Mixture WoodAsh = new(
                    new (HomogeneousReference, decimal)[]
                    {
                    (AmorphousCarbon.GetHomogeneousReference(), 0.59m),
                    (CalciumCarbonate.GetHomogeneousReference(), 0.3m),
                    (PotassiumCarbonate.GetHomogeneousReference(), 0.1m),
                    (PhosphoricAcid.GetHomogeneousReference(), 0.01m),
                    },
                    "Wood Ash",
                    densitySolid: 827);

            #endregion Organics

            #endregion Mixtures

            /// <summary>
            /// <para>
            /// Registers the default set of <see cref="ISubstance"/>s in the given <paramref
            /// name="dataStore"/>.
            /// </para>
            /// <para>
            /// Also sets that <paramref name="dataStore"/> as the <see
            /// cref="DataStore"/> used for all <see cref="ISubstance"/> registrations.
            /// </para>
            /// </summary>
            /// <param name="dataStore">
            /// The <see cref="IDataStore"/> in which to record <see cref="ISubstance"/> instances.
            /// </param>
            /// <remarks>
            /// <para>
            /// Note that any <see cref="ISubstance"/>s already manually registered prior to calling
            /// this method will be "orphaned" in the <see cref="IDataStore"/> formerly associated
            /// with <see cref="DataStore"/>, since the lookup methods only work with the
            /// current <see cref="DataStore"/>.
            /// </para>
            /// <para>
            /// Manual retrieval of <see cref="ISubstance"/> instances via the usual methods of <see
            /// cref="IDataStore"/> would still be possible on a former <see cref="IDataStore"/>,
            /// but the internal <see cref="InMemoryDataStore"/>, if it had been used, would no
            /// longer be accesible.
            /// </para>
            /// </remarks>
            public static void RegisterAll(IDataStore dataStore)
            {
                DataStore = dataStore;
                AlwaysUseDataStore = true;
                RegisterAllInternal(dataStore);
                _InternalDataStore = new InMemoryDataStore();
            }

            internal static void RegisterAllInternal(IDataStore dataStore)
            {
                #region Chemicals

                #region Atmospheric

                Ammonia.Register(dataStore);
                AmmoniumHydrosulfide.Register(dataStore);
                HydrogenSulfide.Register(dataStore);
                Phosphine.Register(dataStore);
                SulphurDioxide.Register(dataStore);

                #endregion Atmospheric

                #region Elements

                Hydrogen.Register(dataStore);
                MetallicHydrogen.Register(dataStore);
                Helium.Register(dataStore);
                Lithium.Register(dataStore);
                Beryllium.Register(dataStore);
                Boron.Register(dataStore);
                AmorphousCarbon.Register(dataStore);
                Diamond.Register(dataStore);
                Nitrogen.Register(dataStore);
                Oxygen.Register(dataStore);
                Ozone.Register(dataStore);
                Fluorine.Register(dataStore);
                Neon.Register(dataStore);
                Sodium.Register(dataStore);
                Magnesium.Register(dataStore);
                Aluminium.Register(dataStore);
                Silicon.Register(dataStore);
                WhitePhosphorus.Register(dataStore);
                RedPhosphorus.Register(dataStore);
                Sulfur.Register(dataStore);
                Chlorine.Register(dataStore);
                Argon.Register(dataStore);
                Potassium.Register(dataStore);
                Calcium.Register(dataStore);
                Scandium.Register(dataStore);
                Titanium.Register(dataStore);
                Vanadium.Register(dataStore);
                Chromium.Register(dataStore);
                Manganese.Register(dataStore);
                Iron.Register(dataStore);
                Cobalt.Register(dataStore);
                Nickel.Register(dataStore);
                Copper.Register(dataStore);
                Zinc.Register(dataStore);
                Gallium.Register(dataStore);
                Germanium.Register(dataStore);
                Arsenic.Register(dataStore);
                Selenium.Register(dataStore);
                Bromine.Register(dataStore);
                Krypton.Register(dataStore);
                Rubidium.Register(dataStore);
                Strontium.Register(dataStore);
                Yttrium.Register(dataStore);
                Zirconium.Register(dataStore);
                Niobium.Register(dataStore);
                Molybdenum.Register(dataStore);
                Technetium.Register(dataStore);
                Ruthenium.Register(dataStore);
                Rhodium.Register(dataStore);
                Palladium.Register(dataStore);
                Silver.Register(dataStore);
                Cadmium.Register(dataStore);
                Indium.Register(dataStore);
                WhiteTin.Register(dataStore);
                GrayTin.Register(dataStore);
                Antimony.Register(dataStore);
                Tellurium.Register(dataStore);
                Iodine.Register(dataStore);
                Xenon.Register(dataStore);
                Caesium.Register(dataStore);
                Barium.Register(dataStore);
                Lanthanum.Register(dataStore);
                Cerium.Register(dataStore);
                Praseodymium.Register(dataStore);
                Neodymium.Register(dataStore);
                Promethium.Register(dataStore);
                Samarium.Register(dataStore);
                Europium.Register(dataStore);
                Gadolinium.Register(dataStore);
                Terbium.Register(dataStore);
                Dysprosium.Register(dataStore);
                Holmium.Register(dataStore);
                Erbium.Register(dataStore);
                Thulium.Register(dataStore);
                Ytterbium.Register(dataStore);
                Lutetium.Register(dataStore);
                Hafnium.Register(dataStore);
                Tantalum.Register(dataStore);
                Tungsten.Register(dataStore);
                Rhenium.Register(dataStore);
                Osmium.Register(dataStore);
                Iridium.Register(dataStore);
                Platinum.Register(dataStore);
                Gold.Register(dataStore);
                Mercury.Register(dataStore);
                Thallium.Register(dataStore);
                Lead.Register(dataStore);
                Bismuth.Register(dataStore);
                Polonium.Register(dataStore);
                Astatine.Register(dataStore);
                Radon.Register(dataStore);
                Francium.Register(dataStore);
                Radium.Register(dataStore);
                Actinium.Register(dataStore);
                Thorium.Register(dataStore);
                Protactinium.Register(dataStore);
                Uranium.Register(dataStore);
                Neptunium.Register(dataStore);
                Plutonium.Register(dataStore);
                Americium.Register(dataStore);
                Curium.Register(dataStore);
                Berkelium.Register(dataStore);
                Californium.Register(dataStore);
                Einsteinium.Register(dataStore);
                Fermium.Register(dataStore);
                Mendelevium.Register(dataStore);
                Nobelium.Register(dataStore);
                Lawrencium.Register(dataStore);
                Rutherfordium.Register(dataStore);
                Dubnium.Register(dataStore);
                Seaborgium.Register(dataStore);
                Bohrium.Register(dataStore);
                Hassium.Register(dataStore);
                Meitnerium.Register(dataStore);
                Darmstadtium.Register(dataStore);
                Roentgenium.Register(dataStore);
                Copernicium.Register(dataStore);
                Nihonium.Register(dataStore);
                Flerovium.Register(dataStore);
                Moscovium.Register(dataStore);
                Livermorium.Register(dataStore);
                Tennessine.Register(dataStore);
                Oganesson.Register(dataStore);

                #endregion Elements

                #region Gems

                Beryl.Register(dataStore);
                Corundum.Register(dataStore);
                Topaz.Register(dataStore);

                #endregion Gems

                #region Hydrocarbons

                Adamantane.Register(dataStore);
                Anthracene.Register(dataStore);
                Benzene.Register(dataStore);
                Biphenyl.Register(dataStore);
                Butane.Register(dataStore);
                CarbonDioxide.Register(dataStore);
                CarbonMonoxide.Register(dataStore);
                Cyclobutane.Register(dataStore);
                Cyclodecane.Register(dataStore);
                Cyclododecane.Register(dataStore);
                Cycloheptane.Register(dataStore);
                Cyclohexane.Register(dataStore);
                Cyclononane.Register(dataStore);
                Cyclooctane.Register(dataStore);
                Cyclopentane.Register(dataStore);
                Cyclopropane.Register(dataStore);
                Cyclotetradecane.Register(dataStore);
                Cyclotridecane.Register(dataStore);
                Cycloundecane.Register(dataStore);
                Cumene.Register(dataStore);
                Decalin.Register(dataStore);
                Decane.Register(dataStore);
                Dodecane.Register(dataStore);
                Durene.Register(dataStore);
                Ethane.Register(dataStore);
                Ethylbenzene.Register(dataStore);
                Ethylene.Register(dataStore);
                Heptane.Register(dataStore);
                Hexadecane.Register(dataStore);
                Hexane.Register(dataStore);
                Hexene.Register(dataStore);
                Indane.Register(dataStore);
                Indene.Register(dataStore);
                Methane.Register(dataStore);
                MXylene.Register(dataStore);
                Naphthalene.Register(dataStore);
                Nonane.Register(dataStore);
                Octane.Register(dataStore);
                OXylene.Register(dataStore);
                Pentadecane.Register(dataStore);
                Pentane.Register(dataStore);
                Phenanthrene.Register(dataStore);
                Propane.Register(dataStore);
                PXylene.Register(dataStore);
                Tetradecane.Register(dataStore);
                Toluene.Register(dataStore);
                Tridecane.Register(dataStore);
                Undecane.Register(dataStore);

                #endregion Hydrocarbons

                #region Ions

                AlphaParticle.Register(dataStore);
                Bicarbonate.Register(dataStore);
                C4Pos.Register(dataStore);
                Ca2Pos.Register(dataStore);
                Cl1Neg.Register(dataStore);
                Cr3Pos.Register(dataStore);
                Fe2Pos.Register(dataStore);
                HydrogenPlasma.Register(dataStore);
                K1Pos.Register(dataStore);
                Mg2Pos.Register(dataStore);
                N5Pos.Register(dataStore);
                Na1Pos.Register(dataStore);
                O2Pos.Register(dataStore);
                S6Pos.Register(dataStore);
                Si4Pos.Register(dataStore);
                Sulfate.Register(dataStore);
                Ti4Pos.Register(dataStore);

                #endregion Ions

                #region Minerals

                Acanthite.Register(dataStore);
                Albite.Register(dataStore);
                Anorthite.Register(dataStore);
                Boehmite.Register(dataStore);
                CalciumCarbonate.Register(dataStore);
                Cassiterite.Register(dataStore);
                Chalcopyrite.Register(dataStore);
                Chromite.Register(dataStore);
                Cinnabar.Register(dataStore);
                Diopside.Register(dataStore);
                Enstatite.Register(dataStore);
                Fayalite.Register(dataStore);
                Ferrosilite.Register(dataStore);
                Forsterite.Register(dataStore);
                Galena.Register(dataStore);
                Gibbsite.Register(dataStore);
                Goethite.Register(dataStore);
                Gypsum.Register(dataStore);
                Hematite.Register(dataStore);
                Hydroxyapatite.Register(dataStore);
                Ilmenite.Register(dataStore);
                Kaolinite.Register(dataStore);
                Magnetite.Register(dataStore);
                Muscovite.Register(dataStore);
                Orthoclase.Register(dataStore);
                PotassiumNitrate.Register(dataStore);
                Pyrite.Register(dataStore);
                SiliconCarbide.Register(dataStore);
                SiliconDioxide.Register(dataStore);
                SodiumChloride.Register(dataStore);
                Sperrylite.Register(dataStore);

                #endregion Minerals

                #region Misc

                AluminiumOxide.Register(dataStore);
                CalciumHydroxide.Register(dataStore);
                CalciumOxide.Register(dataStore);
                IronSulfide.Register(dataStore);
                LeadOxide.Register(dataStore);
                Lead206Oxide.Register(dataStore);
                PhosphoricAcid.Register(dataStore);
                SodiumOxide.Register(dataStore);
                SulfuricAcid.Register(dataStore);
                Water.Register(dataStore);
                ZincSulfide.Register(dataStore);
                UraniumDioxide.Register(dataStore);
                Uranium235Dioxide.Register(dataStore);

                #endregion Misc

                #region Organics

                Benzothiophene.Register(dataStore);
                Cellulose.Register(dataStore);
                Chitin.Register(dataStore);
                Collagen.Register(dataStore);
                Dibenzothiophene.Register(dataStore);
                Elastin.Register(dataStore);
                Ethanol.Register(dataStore);
                Fructose.Register(dataStore);
                Galactose.Register(dataStore);
                Glucose.Register(dataStore);
                Glycogen.Register(dataStore);
                IsopropylAlcohol.Register(dataStore);
                Lactose.Register(dataStore);
                Lignin.Register(dataStore);
                Maltose.Register(dataStore);
                Methanol.Register(dataStore);
                PotassiumCarbonate.Register(dataStore);
                SodiumCarbonate.Register(dataStore);
                Sucrose.Register(dataStore);
                Triolein.Register(dataStore);

                #endregion Organics

                #region Plastics

                Nylon.Register(dataStore);
                Polycarbonate.Register(dataStore);
                Polyester.Register(dataStore);
                Polyethylene.Register(dataStore);
                Polypropylene.Register(dataStore);
                Polystyrene.Register(dataStore);
                PolyvinylChloride.Register(dataStore);

                #endregion Plastics

                #endregion Chemicals

                #region HomogeneousSubstances

                #region Cosmic

                Fuzzball.Register(dataStore);
                NeutronDegenerateMatter.Register(dataStore);

                #endregion Cosmic

                #region Organics

                Blood.Register(dataStore);
                Flesh.Register(dataStore);
                Keratin.Register(dataStore);
                Protein.Register(dataStore);

                #endregion Organics

                #endregion HomogeneousSubstances

                #region Solutions

                #region Alloys

                Brass.Register(dataStore);
                Bronze.Register(dataStore);
                CarbonSteel.Register(dataStore);
                Ferrochrome.Register(dataStore);
                IronNickelAlloy.Register(dataStore);
                StainlessSteel.Register(dataStore);

                #endregion Alloys

                #region Aqueous

                Seawater.Register(dataStore);

                #endregion Aqueous

                #region Artificial

                Cement.Register(dataStore);
                CottonCloth.Register(dataStore);
                Leather.Register(dataStore);
                Paper.Register(dataStore);
                SodaLimeGlass.Register(dataStore);
                Stoneware.Register(dataStore);

                #endregion Artificial

                #region Gems

                Emerald.Register(dataStore);
                Ruby.Register(dataStore);
                Sapphire.Register(dataStore);

                #endregion Gems

                #region Hydrocarbons

                Vitrinite.Register(dataStore);

                #endregion Hydrocarbons

                #region Minerals

                Olivine.Register(dataStore);
                CosmicDust.Register(dataStore);
                Orthopyroxene.Register(dataStore);
                Plagioclase.Register(dataStore);
                Sphalerite.Register(dataStore);
                Uraninite.Register(dataStore);

                #endregion Minerals

                #region Organics

                Wood.Register(dataStore);
                WoodSmoke.Register(dataStore);

                #endregion Organics

                #endregion Solutions

                #region Mixtures

                #region Artificial

                Brick.Register(dataStore);
                Concrete.Register(dataStore);
                Earthenware.Register(dataStore);
                ReinforcedConcrete.Register(dataStore);

                #endregion Artificial

                #region Cosmic

                InterplanetaryMedium.Register(dataStore);
                InterstellarMedium.Register(dataStore);
                IntraclusterMedium.Register(dataStore);
                IonizedCloud.Register(dataStore);
                MolecularCloud.Register(dataStore);
                WarmHotIntergalacticMedium.Register(dataStore);

                #endregion Cosmic

                #region Hydrocarbons

                Anthracite.Register(dataStore);
                BituminousCoal.Register(dataStore);
                Diesel.Register(dataStore);
                Gasoline.Register(dataStore);
                Kerosine.Register(dataStore);
                NaturalGas.Register(dataStore);
                Petroleum.Register(dataStore);

                #endregion Hydrocarbons

                #region Minerals

                BallClay.Register(dataStore);
                Basalt.Register(dataStore);
                Bauxite.Register(dataStore);
                Granite.Register(dataStore);
                Loam.Register(dataStore);
                Peridotite.Register(dataStore);
                Sandstone.Register(dataStore);
                Silt.Register(dataStore);

                #endregion Minerals

                #region Misc

                FlyAsh.Register(dataStore);

                #endregion Misc

                #region Organics

                AdiposeTissue.Register(dataStore);
                Bone.Register(dataStore);
                EpithelialTissue.Register(dataStore);
                MuscleTissue.Register(dataStore);
                NervousTissue.Register(dataStore);
                Tooth.Register(dataStore);
                WoodAsh.Register(dataStore);

                #endregion Organics

                #endregion Mixtures
            }
        }

        static Substances() => All.RegisterAllInternal(_InternalDataStore);
    }
}
