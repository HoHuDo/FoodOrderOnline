using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class MonAnPaginationVM
    {

        public IEnumerable<MonAn> MonAns { get; set; }


        public PaginationInfo Pagination { get; set; }

        public List<int> SelectedDanhMucIds { get; set; } = new List<int>();
    }
}