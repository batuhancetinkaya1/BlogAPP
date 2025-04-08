using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BlogApp.Services
{
    public interface INotificationService
    {
        void Success(string message);
        void Error(string message);
        void Info(string message);
        void Warning(string message);
    }

    public class TempDataNotificationService : INotificationService
    {
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TempDataNotificationService(
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Success(string message)
        {
            AddNotification("success", message);
        }

        public void Error(string message)
        {
            AddNotification("error", message);
        }

        public void Info(string message)
        {
            AddNotification("info", message);
        }

        public void Warning(string message)
        {
            AddNotification("warning", message);
        }

        private void AddNotification(string type, string message)
        {
            var tempData = _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext);
            tempData[type] = message;
        }
    }
} 