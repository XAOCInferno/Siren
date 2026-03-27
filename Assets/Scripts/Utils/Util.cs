using System.Collections.Generic;

namespace Utils
{
    public static class Util
    {
        /// <summary>
        /// Remove any duplicate entries in a list. Note: this is slow for larger lists.
        /// </summary>
        /// <param name="listOfItems">The list we wish to check for duplicates within.</param>
        public static void RemoveDuplicates<T>(this List<T> listOfItems)
        {
            for (int a = 0; a < listOfItems.Count; a++)
            {
                for (int b = listOfItems.Count - 1; b > a; b--)
                {
                    if (listOfItems[a].Equals(listOfItems[b]))
                    {
                        listOfItems.RemoveAt(b);
                    }
                }
            }
        }
    }
}