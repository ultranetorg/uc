using Uuc.Models;

namespace Uuc.Services;

public class DigitalIdentitiesService : IDigitalIdentitiesService
{
	private readonly IList<DigitalIdentity> _digitalIdentities = new List<DigitalIdentity>()
	{
		new DigitalIdentity
		{
			Name = "Identity 1",
			AvatarBase64String =
				@"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAGUExURQAAAP///6XZn90AAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVEjH5dRhDoAgCAZQuIHc/7IlBtqc5uZHW4sfxfBtllD0uRCNFAdYLKJAXR88xgrIF01iQKmy1BwHuBxMLko6ASfHxT0BO5huC/YFDGgbcXUUBzQZATuHOSBXPtUKul4DgBXbHAjotv9gopaARQh44/+wHzYgPHuJTZC/zeaOB6h5CAW/DqIDvsco1efHjfoAAAAASUVORK5CYII="
		},
		new DigitalIdentity
		{
			Name = "Id 2",
			AvatarBase64String =
				@"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAGUExURQAAAAD//xtBqzAAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAECSURBVEjHpZULEoQgDEPbGzT3v+w2gf0KdBFGRkffJP0A2vcAELYaFeB9LgDEEeDBGLACUsHOgNLiNIZWxUUtC4BtyDrBJv3ILzmdhOMeoGYTmPSjAij7BjAE8jszyBzuAyH18MAkhlMADIIPHoNi10CoDgRaJr8DOAb4yl/Lbqwgzl/4LhCy7324AbwTcJV6AGToWitB4CpRAyoDrFlgrsB76KTZBng9LRLwfSAEdIt0uyp4ZeF/WGBtoZavLAog2vZXw2NkUQIfFjwjZgra4VTAPtA3TlZbGVwXbQ/Su8INgBaMfmpRAmmBtUU76qXQOroL8H8loJ90ZqMw26raAcwegvEiDIrlgV0AAAAASUVORK5CYII="
		},
		new DigitalIdentity
		{
			Name = "Person 3",
			AvatarBase64String =
				@"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJUExURQAAAP///98AAPUT8XwAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC9SURBVEjHtZUJDoQgDEWBE1RPYLj/IS0tLUuYEaH+hIDlKd2ITuWPIsAHcJ02gXqXCDTBG6DfRyWjIXCMhC5oJLvAwIEFAHTWNU1gCYyGFeA5JoltA4D6qJwpY6AZHwA/6k19uQ5IZ9sAUlXgDpe3jACOKBt8GiVZbwA+oW0eY2DfySdA72MGQyQ5XMwCnU5joL6LOVmATlzBHAAOU4AU6iwgny3ppiPi58CjDwuA9CL9BppETQC75f4HhHgD+Blknh84p+0AAAAASUVORK5CYII=",
		},
		new DigitalIdentity
		{
			Name = "4",
			AvatarBase64String =
				@"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAGUExURQAAAPj4AMLV8BIAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACHSURBVEjHzZRBDsAgCAThB/X/n20tUhC1hyZL5WDCMiYCAplxeeygmYEB7lUeoP8Bvr2QBTsEDlS/BVUQiJuQCIRC2Q04UA8X1mtNupztAMvN8kgEhm7GRyYAYeIXWSQA2FJ/B8K3F+1lLrBAid1MBGTjuSWo1u9JMOCHmWbhPQCFlkEcQHQC/5JExIrddxYAAAAASUVORK5CYII=",
		},
		new DigitalIdentity
		{
			Name = "Max 5",
			AvatarBase64String =
				@"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAGUExURQAAAAD4AHkC0noAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAB5SURBVEjHxdNbCsAgDETRbCH732yhopma2L9cA6VNPcLgw0b5W1Z2DBjt/BlvFsQzv6KngMaayySLBQAd1AESuGuwdJoA8A11mN8K9nApLAKijcMrVwgBK44pXRwAeVe2kDCIi1sWCg6kHeiB/Z3fBnSjSgKAi2X2AG1BOFFksMPAAAAAAElFTkSuQmCC",
		}
	};

	public async Task<IList<DigitalIdentity>?> ListAllAsync(CancellationToken cancellationToken = default)
	{
		return _digitalIdentities;
	}

	public async Task<DigitalIdentity?> FindAsync(string name, CancellationToken cancellationToken = default)
	{
		Guard.IsNotEmpty(name);

		return _digitalIdentities.FirstOrDefault(x => x.Name == name);
	}
}
