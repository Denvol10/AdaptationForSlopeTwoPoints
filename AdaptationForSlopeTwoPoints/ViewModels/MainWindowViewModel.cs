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
        private string _title = "Адаптация под уклон 1 точка";

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

        #region Линия на поверхности
        private string _roadLineElemIds1;

        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => Set(ref _roadLineElemIds1, value);
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

        #region Получение линии на поверхности дороги
        public ICommand GetRoadLine { get; }

        private void OnGetRoadLineCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine1();
            RoadLineElemIds1 = RevitModel.RoadLineElemIds1;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLineCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Перенос точки ручки формы
        public ICommand MoveShapeHandlePointCommand { get; }

        private void OnMoveShapeHandlePointCommandExecuted(object parameter)
        {
            RevitModel.MoveShapeHandlePoint();
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
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Команды
            GetAdaptiveProfiles = new LambdaCommand(OnGetAdaptiveProfilesCommandExecuted, CanGetAdaptiveProfilesCommandExecute);
            GetRoadLine = new LambdaCommand(OnGetRoadLineCommandExecuted, CanGetRoadLineCommandExecute);
            MoveShapeHandlePointCommand = new LambdaCommand(OnMoveShapeHandlePointCommandExecuted, CanMoveShapeHandlePointCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel()
        { }
        #endregion
    }
}
