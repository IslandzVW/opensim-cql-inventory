/*
    Copyright (c) 2014, InWorldz, LLC
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    * Neither the name of opensim-cql-inventory nor the names of its
      contributors may be used to endorse or promote products derived from
      this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQLInventoryBackend
{
    /// <summary>
    /// Represents a single item in an inventory folder
    /// </summary>
    public class InventoryItem
    {
        public Guid ItemId { get; set; }
        public Guid FolderId { get; set; }
        public string Name { get; set; }
        public Guid AssetId { get; set; }
        public int BasePermissions { get; set; }
        public int CreationDate { get; set; }
        public Guid CreatorId { get; set; }
        public int CurrentPermissions { get; set; }
        public string Description { get; set; }
        public int EveryonePermissions { get; set; }
        public int Flags { get; set; }
        public Guid GroupId { get; set; }
        public bool GroupOwned { get; set; }
        public int GroupPermissions { get; set; }
        public int InventoryType { get; set; }
        public int NextPermissions { get; set; }
        public Guid OwnerId { get; set; }
        public int SaleType { get; set; }
    }
}
