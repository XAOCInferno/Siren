using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;

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


        //Use this as a function return value for a function which would normally return a bool (success or fail)
        public struct ActionResult
        {
            public readonly bool isSuccess;
            [CanBeNull] readonly string msg;

            public ActionResult(bool isSuccess, [CanBeNull] string msg = null)
            {
                this.isSuccess = isSuccess;
                this.msg = msg;
                //Log message if we have it
                if (msg == null) return;
                if (isSuccess)
                {
                    //Success, Log
                    DebugSystem.Log(msg);
                }
                else
                {
                    //Fail, Warn. We do not need to error as most failures are safe. If it is not safe then we can handle on a need-to basis
                    DebugSystem.Warn(msg);
                }
            }
        }
    }
}