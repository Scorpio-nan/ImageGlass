﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2025 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.

Author: Kevin Routley (aka fire-eggs)
*/
using System.Runtime.InteropServices;
using System.Text;

namespace ImageGlass.Base.DirectoryComparer;


public static class ExplorerSortOrder
{
    /// <summary>
    /// Convert an Explorer column name to one of our currently available sorting orders.
    /// </summary>
    private static readonly Dictionary<string, ImageOrderBy> SortTranslation = new()
    {
        { "System.ItemTypeText", ImageOrderBy.Extension },
        { "System.FileExtension", ImageOrderBy.Extension },
        { "System.FileName", ImageOrderBy.Name },
        { "System.ItemNameDisplay", ImageOrderBy.Name },
        { "System.Size", ImageOrderBy.FileSize },
        { "System.DateCreated", ImageOrderBy.DateCreated },
        { "System.DateAccessed", ImageOrderBy.DateAccessed },
        { "System.DateModified", ImageOrderBy.DateModified },

        { "System.Photo.DateTaken", ImageOrderBy.ExifDateTaken },
        { "System.Rating", ImageOrderBy.ExifRating },
    };


    [DllImport("ExplorerSortOrder.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetExplorerSortOrder")]
    private static extern int GetExplorerSortOrder(string folderPath, ref StringBuilder columnName, int columnNameMaxLen, ref int isAscending);


    /// <summary>
    /// <para>
    /// Determines the sorting order of a Windows Explorer window which matches
    /// the given file path.
    /// </para>
    /// "Failure" situations are:
    /// <list type="number">
    ///     <item>Unable to find an open Explorer window matching the file path</item>
    ///     <item>The Explorer sort order doesn't match one of our existing sort orders</item>
    /// </list>
    /// </summary>
    /// <param name="fullPath">full path to file/folder in question</param>
    /// <param name="loadOrder">the resulting sort order or null</param>
    /// <param name="isAscending">the resulting sort direction or null</param>
    /// <returns>false on failure - out parameters will be null!</returns>
    public static bool GetExplorerSortOrder(string fullPath, out ImageOrderBy? loadOrder, out bool? isAscending)
    {
        // assume failure
        loadOrder = null;
        isAscending = null;

        try
        {
            // if fullPath is a drive root (e.g. "L:\") then Path.GetDirectoryName returns null
            var folderPath = Path.GetDirectoryName(fullPath) ?? fullPath;

            var sb = new StringBuilder(200); // arbitrary length should fit any
            int sortResult;
            var ascend = -1;

            sortResult = GetExplorerSortOrder(folderPath, ref sb, sb.Capacity, ref ascend);

            if (sortResult != 0) // failure
            {
                return false;
            }

            // Success! Attempt to translate the Explorer column to our supported
            // sort order values.
            var column = sb.ToString();
            if (SortTranslation.TryGetValue(column, out ImageOrderBy value))
            {
                loadOrder = value;
            }

            isAscending = ascend > 0;

            return loadOrder != null; // will be false on not-yet-supported column
        }
        catch
        {
            return false; // failure
        }
    }
}

