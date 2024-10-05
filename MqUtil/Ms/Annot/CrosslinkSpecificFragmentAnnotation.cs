namespace MqUtil.Ms.Annot {

    /// <summary>
    /// This enum class is used while writing identifications for the CrosslinkMsms table and then later during the visualization of CSMs.
    /// The purpose of this enum class is to visualize the same fragment ion type which contains different cross link products.
    /// For example, a fragment can contain an entire paired-peptide, then it is "Pep" (peptide).
    /// If a fragment contains a shorter residual of MS-cleavable cross linker, then it is "S" (shorter).
    /// In case it has a longer residual of MS-cleavable cross linker, then is "L" (longer).
    /// If it is a fragment with monolink fragment, then it is "ML" (mono-link)
    /// </summary>
    public enum CrossLinkSpecificFragmentAnnotation {
        None, 
        S,
        L, 
        ML,
        Pep 
    }
}