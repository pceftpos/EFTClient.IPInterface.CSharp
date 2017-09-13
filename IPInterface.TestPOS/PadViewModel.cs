
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public class PadViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        string _filename = "temp.json";
        ExternalDataList _items = new ExternalDataList();
        public ExternalDataList UpdatedExternalData
        {
            set
            {
                _items = value;
            }
            get
            {
                _items.Clear();
                _items.AddRange(PadContentList);

                return _items;
            }
        }

        public ObservableCollection<ExternalData> PadContentList { get; set; } = new ObservableCollection<ExternalData>();
        public ObservableCollection<FieldTag> PadFieldList { get; set; } = new ObservableCollection<FieldTag>();

        string _padName = string.Empty;
        public string PadName
        {
            get
            {
                return _padName;
            }
            set
            {
                _padName = value;
                NotifyPropertyChanged("PadName");
            }
        }

        string _padValue = string.Empty;
        public string PadValue
        {
            get
            {
                return _padValue;
            }
            set
            {
                _padValue = value;
                NotifyPropertyChanged("PadValue");
            }
        }

        string _padTagName = string.Empty;
        public string PadTagName
        {
            get
            {
                return _padTagName;
            }
            set
            {
                _padTagName = value;
                NotifyPropertyChanged("PadTagName");
            }
        }

        string _padTagValue = string.Empty;
        public string PadTagValue
        {
            get
            {
                return _padTagValue;
            }
            set
            {
                _padTagValue = value;
                NotifyPropertyChanged("PadTagValue");
            }
        }

        bool _editMode = false;
        public bool EditMode
        {
            get
            {
                return _editMode;
            }
            set
            {
                _editMode = value;
                NotifyPropertyChanged("EditMode");
            }
        }

        int _currentPadIndex = -1;

        public PadViewModel(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                _filename = filename;
                Load();
            }

            if (_items != null)
            {
                PadContentList = new ObservableCollection<ExternalData>(_items);
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(UpdatedExternalData.ToArray(), Formatting.Indented);
                File.WriteAllText(_filename, json);
            }
            catch
            {
            }
        }

        private void Load()
        {
            try
            {
                ExternalDataList list = (ExternalDataList)JsonConvert.DeserializeObject(File.ReadAllText(_filename), typeof(ExternalDataList));
                if (list != null && list.Count > 0)
                {
                    _items.Clear();
                    _items.AddRange(list);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public bool AddPadContentFunc()
        {
            if (string.IsNullOrEmpty(PadName))
                return false;

            if (PadContentList.ToList().Exists(x => x.Name.Equals(PadName)))
                return false;

            PadContentList.Add(new ExternalData(PadName, PadValue));
            PadName = string.Empty;
            PadValue = string.Empty;

            return true;
        }


        public bool AddPadTagFunc()
        {
            if (string.IsNullOrEmpty(PadTagName))
                return false;

            if (PadFieldList.ToList().Exists(x => x.Name.Equals(PadTagName)))
                return false;
            
            PadFieldList.Add(new FieldTag(PadTagName, PadTagValue));
            PadTagName = string.Empty;
            PadTagValue = string.Empty;

            return true;
        }

        public bool UpdatePadContentFunc(int index)
        {
            if (string.IsNullOrEmpty(PadName) || index < 0)
                return false;

            PadContentList[index].Name = PadName;
            PadContentList[index].Value = PadValue;
            PadContentList[index].Fields.Clear();

            return true;
        }

        public bool UpdatePadTagFunc(int index)
        {
            if (string.IsNullOrEmpty(PadTagName) || index < 0)
                return false;

            PadFieldList[index].Name = PadTagName;
            PadFieldList[index].Data = PadTagValue;
            return true;
        }

        public RelayCommand DeletePadContent
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    var index = (int)o;
                    if (index < 0)
                        return;

                    PadContentList.RemoveAt(index);
                    PadName = string.Empty;
                    PadValue = string.Empty;
                });
            }
        }

        public RelayCommand DeletePadTag
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o == null)
                        return;

                    int index = (int)o;
                    if (index < 0)
                        return;

                    PadFieldList.RemoveAt(index);

                    PadTagName = string.Empty;
                    PadTagValue = string.Empty;
                });
            }
        }

        public RelayCommand AddPadTag
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (string.IsNullOrEmpty(PadName))
                        return;

                    PadFieldList.Add(new FieldTag() { Name = PadName, Data = PadValue });

                    PadName = string.Empty;
                    PadValue = string.Empty;
                });
            }
        }

        public RelayCommand LoadEditor
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o == null)
                        return;

                    _currentPadIndex = (int)o;
                    if (_currentPadIndex < 0)
                        return;

                    PadFieldList.Clear();
                    PadTagName = string.Empty;
                    PadTagValue = string.Empty;

                    if (PadContentList[_currentPadIndex].Fields != null)
                    {
                        PadContentList[_currentPadIndex].Fields.ForEach(x =>
                        {
                            PadFieldList.Add(new FieldTag(x.Name, x.Data));
                        });
                    }

                    EditMode = true;
                });
            }
        }

        public bool SavePadFieldFunc()
        {
            if (_currentPadIndex < 0)
                return false;

            var value = string.Empty;
            PadFieldList.ToList().ForEach(x => value += x.Name + x.Data);

            PadContentList[_currentPadIndex].Value = value;
            PadContentList[_currentPadIndex].Fields.Clear();
            PadContentList[_currentPadIndex].Fields.AddRange(PadFieldList);

            PadValue = value;

            EditMode = false;

            return true;
        }

    }
}
