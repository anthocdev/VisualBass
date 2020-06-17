using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual.cs.Models
{
    public class PlaylistModel : ObservableCollection<MetaModel>
    {
        //Overriding Add method to avoid duplicate files in playlist
        protected override void InsertItem(int index, MetaModel item)
        {
            if (this.Any(c => c.File == item.File)) { return; } //Preventing duplicates
            base.InsertItem(index, item);
        }
    }
}
