using System;
using System.Collections.ObjectModel;

namespace Raydreams.Autodesk.Extensions
{
    public static class CollectionsExtensions
    {
        /// <summary>Add multiple items to an observable collection</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int AddRange<T>( this ObservableCollection<T> coll, IEnumerable<T> items )
        {
            int results = 0;

            foreach ( T item in items )
            {
                coll.Add( item );
                ++results;
            }

            return results;
        }
    }
}

