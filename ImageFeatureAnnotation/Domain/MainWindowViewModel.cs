using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ImageFeatureAnnotation.Domain
{
    public class MainWindowViewModel : ViewModelBase
    {
        MainWindow? WIN;
        public ObservableCollection<double[]>? Data { get; }
        public MainWindowViewModel()
        {

        }
        ObservableCollection<SelectableFiles> _FileList;
        public ObservableCollection<SelectableFiles> FileList
        {
            set => SetProperty(ref _FileList, value);
            get => _FileList;
        }
        ObservableCollection<SelectableFeature> _FeatureList;
        public ObservableCollection<SelectableFeature> FeatureList
        {
            set => SetProperty(ref _FeatureList, value);
            get => _FeatureList;
        }
        ObservableCollection<int> _CatFeatures;
        public ObservableCollection<int> CatFeatures
        {
            set;
            get;
        } = new ObservableCollection<int>(new List<int> { 0, 1, 2, 3, 4, 5, 6,7,8,9,10,11,12,13,14,15,16,17,18,19 });
    }
}
