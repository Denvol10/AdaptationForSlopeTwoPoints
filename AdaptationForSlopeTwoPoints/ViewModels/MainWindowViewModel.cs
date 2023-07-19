using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AdaptationForSlopeTwoPoints.Infrastructure;

namespace AdaptationForSlopeTwoPoints.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Адаптация под уклон 2 точки";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Адаптивные профили
        private string _adaptiveProfileElemIds;

        public string AdaptiveProfileElemIds
        {
            get => _adaptiveProfileElemIds;
            set => Set(ref _adaptiveProfileElemIds, value);
        }
        #endregion

        #region Линия на поверхности 1
        private string _roadLineElemIds1;

        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => Set(ref _roadLineElemIds1, value);
        }
        #endregion

        #region Линия на поверхности 2
        private string _roadLineElemIds2;

        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => Set(ref _roadLineElemIds2, value);
        }
        #endregion

        #region Команды

        #region Получение адаптивных профилей
        public ICommand GetAdaptiveProfiles { get; }

        private void OnGetAdaptiveProfilesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetAdaptiveProfiles();
            AdaptiveProfileElemIds = RevitModel.AdaptiveProfileElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetAdaptiveProfilesCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии 1 на поверхности дороги
        public ICommand GetRoadLine1Command { get; }

        private void OnGetRoadLine1CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine1();
            RoadLineElemIds1 = RevitModel.RoadLineElemIds1;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLine1CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии 2 на поверхности дороги
        public ICommand GetRoadLine2Command { get; }

        private void OnGetRoadLine2CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine2();
            RoadLineElemIds2 = RevitModel.RoadLineElemIds2;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLine2CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Перенос точки ручки формы
        public ICommand MoveShapeHandlePointCommand { get; }

        private void OnMoveShapeHandlePointCommandExecuted(object parameter)
        {
            RevitModel.MoveShapeHandlePoint();
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanMoveShapeHandlePointCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        public void SaveSettings()
        {
            Properties.Settings.Default["AdaptiveProfileElemIds"] = AdaptiveProfileElemIds;
            Properties.Settings.Default["RoadLineElemIds1"] = RoadLineElemIds1;
            Properties.Settings.Default.Save();
        }

        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация значения элементам адаптивных профилей из Settings
            if (!(Properties.Settings.Default["AdaptiveProfileElemIds"] is null))
            {
                string profileElementIdsInSettings = Properties.Settings.Default["AdaptiveProfileElemIds"].ToString();
                if(RevitModel.IsFamilyInstancesExistInModel(profileElementIdsInSettings) && !string.IsNullOrEmpty(profileElementIdsInSettings))
                {
                    AdaptiveProfileElemIds = profileElementIdsInSettings;
                    RevitModel.GetFamilyInstancesBySettings(profileElementIdsInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии 1 на поверхности из Settings
            if (!(Properties.Settings.Default["RoadLineElemIds1"] is null))
            {
                string roadLine1InSettings = Properties.Settings.Default["RoadLineElemIds1"].ToString();
                if(RevitModel.IsLinesExistInModel(roadLine1InSettings) && !string.IsNullOrEmpty(roadLine1InSettings))
                {
                    RoadLineElemIds1 = roadLine1InSettings;
                    RevitModel.GetRoadLine1BySettings(roadLine1InSettings);
                }
            }
            #endregion

            #region Команды
            GetAdaptiveProfiles = new LambdaCommand(OnGetAdaptiveProfilesCommandExecuted, CanGetAdaptiveProfilesCommandExecute);
            GetRoadLine1Command = new LambdaCommand(OnGetRoadLine1CommandExecuted, CanGetRoadLine1CommandExecute);
            GetRoadLine2Command = new LambdaCommand(OnGetRoadLine2CommandExecuted, CanGetRoadLine2CommandExecute);
            MoveShapeHandlePointCommand = new LambdaCommand(OnMoveShapeHandlePointCommandExecuted, CanMoveShapeHandlePointCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel()
        { }
        #endregion
    }
}
