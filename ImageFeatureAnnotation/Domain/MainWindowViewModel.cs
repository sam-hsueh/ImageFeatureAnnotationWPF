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
            get
            {
                var arr = new ObservableCollection<int>();
                for (int i = 0; i < 100; i++)
                    arr.Add(i);
                return arr;
            }
        }
    }
}
