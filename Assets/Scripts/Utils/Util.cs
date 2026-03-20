using System.Collections.Generic;

namespace Utils
{
    public static class Util
    {
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