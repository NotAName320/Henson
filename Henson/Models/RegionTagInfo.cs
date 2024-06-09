using System.Collections.Generic;

namespace Henson.Models;

public class RegionTagInfo(
    List<(string name, int type)> embassies,
    List<string> tags,
    List<string> rmbIds,
    List<string> dispatchIds)
{
    public List<(string name, int type)> Embassies { get; set; } = embassies;
    public List<string> Tags { get; set; } = tags;
    public List<string> RmbIds { get; set; } = rmbIds;
    public List<string> DispatchIds { get; set; } = dispatchIds;
}