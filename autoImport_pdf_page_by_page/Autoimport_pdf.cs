using Ascon.Pilot.SDK;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace autoImport_pdf_page_by_page
{
    [Export(typeof(IAutoimportHandler))]
    public class AutoimportHandler : IAutoimportHandler
    {
        private readonly IObjectsRepository _repository;
        private static Guid _parentId = SystemObjectIds.RootObjectId;

        private readonly IObjectModifier _modifier;
        private readonly IPilotDialogService _dialogService;

        [ImportingConstructor]
        public AutoimportHandler(IObjectModifier modifier, IPilotDialogService dialogService, IObjectsRepository repository)
        {
            _repository = repository;
            _modifier = modifier;
            _dialogService = dialogService;
        }
        public bool Handle(string filePath, string sourceFilePath, AutoimportSource autoimportSource)
        {
            try
            {
                var selection = _dialogService.ShowDocumentsSelectorDialogAndNavigate(_parentId).ToList();
                var folder = selection.First();
                var document = selection.First();
                //if (!document.Type.HasFiles)
                //    MessageBox.Show("Error", "Selected element can not have files", MessageBoxButton.OK, MessageBoxImage.Error);

                _parentId = document.ParentId;
                var type = _repository.GetType("document");
                var message = "Auto-imported from " + Localize(autoimportSource);
                _modifier
                    .Create(folder, type)
                    .SetAttribute("name","samplefile")
                    .AddFile(filePath);
                _modifier.Apply();
                return true;
            }
            finally
            {
                File.Delete(filePath);
            }
        }
        private string Localize(AutoimportSource autoimportSource)
        {
            switch (autoimportSource)
            {
                case AutoimportSource.Unknown:
                    return "Unknown";
                case AutoimportSource.PilotXps:
                    return "Pilot XPS printer";
                case AutoimportSource.UserFolder:
                    return "user auto-import directory";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
