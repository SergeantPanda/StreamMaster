﻿using System.ComponentModel.DataAnnotations.Schema;

namespace StreamMaster.Domain.Models;

public class ChannelGroup : BaseEntity
{
    public static string APIName => "ChannelGroups";
    public bool IsSystem { get; set; }
    public bool IsHidden { get; set; } = false;
    public bool IsReadOnly { get; set; }
    [Column(TypeName = "citext")]
    public string Name { get; set; } = string.Empty;
    [Column(TypeName = "citext")]
    public string RegexMatch { get; set; } = string.Empty;
    public int ActiveCount { get; set; }
    public int TotalCount { get; set; }
    public int HiddenCount { get; set; }
}
