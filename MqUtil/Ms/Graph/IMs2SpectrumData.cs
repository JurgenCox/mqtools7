namespace MqUtil.Ms.Graph {
    public interface IMs2SpectrumData{
        SpectrumInfo ProvideMs2Data(int settingsSelectedFileIndex, bool positive, int first, bool isFaims, int faimsIndex);

        int GetMs2Count(int selectedFileIndex, bool positive);
    }
}