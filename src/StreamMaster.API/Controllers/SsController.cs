﻿using System.Text;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamMaster.Application.Common.Extensions;
using StreamMaster.Application.StreamGroups.Queries;
using StreamMaster.Domain.Enums;

namespace StreamMaster.API.Controllers;

[V1ApiController("s")]
public class SsController(ISender Sender, IStreamGroupService streamGroupService, IFeatureFlagService featureFlagService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [Route("{streamGroupProfileId}")]
    [Route("{streamGroupProfileId}/capability")]
    [Route("{streamGroupProfileId}/device.xml")]
    public async Task<IActionResult> GetStreamGroupCapability(int streamGroupProfileId)
    {
        if (!featureFlagService.IsFeatureEnabled(FeatureFlags.ShortLinks))
        {
            return StatusCode(403, $"{nameof(FeatureFlags.ShortLinks)} are not enabled.");
        }

        if (HttpContext.Request == null)
        {
            return NotFound();
        }

        string xml = await streamGroupService.GetStreamGroupCapabilityAsync(streamGroupProfileId, HttpContext.Request).ConfigureAwait(false);
        return new ContentResult
        {
            Content = xml,
            ContentType = "application/xml",
            StatusCode = 200
        };
    }

    [HttpGet]
    [Authorize(Policy = "SGLinks")]
    [Route("{streamGroupProfileId}/discover.json")]
    public async Task<IActionResult> GetStreamGroupDiscover(int streamGroupProfileId)
    {
        if (!featureFlagService.IsFeatureEnabled(FeatureFlags.ShortLinks))
        {
            return StatusCode(403, $"{nameof(FeatureFlags.ShortLinks)} are not enabled.");
        }

        if (HttpContext.Request == null)
        {
            return NotFound();
        }

        string json = await streamGroupService.GetStreamGroupDiscoverAsync(streamGroupProfileId, HttpContext.Request).ConfigureAwait(false);

        return new ContentResult
        {
            Content = json,
            ContentType = "text/json",
            StatusCode = 200
        };
    }

    [HttpGet]
    [Authorize(Policy = "SGLinks")]
    [Route("{streamGroupProfileId}/lineup.json")]
    public async Task<IActionResult> GetStreamGroupLineup(int streamGroupProfileId)
    {
        if (!featureFlagService.IsFeatureEnabled(FeatureFlags.ShortLinks))
        {
            return StatusCode(403, $"{nameof(FeatureFlags.ShortLinks)} are not enabled.");
        }

        if (HttpContext.Request == null)
        {
            return NotFound();
        }
        string json = await streamGroupService.GetStreamGroupLineupAsync(streamGroupProfileId, true).ConfigureAwait(false);
        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200
        };
    }

    [HttpGet]
    [Authorize(Policy = "SGLinks")]
    [Route("{streamGroupProfileId}/lineup_status.json")]
    public Task<IActionResult> GetStreamGroupLineupStatus(int streamGroupProfileId)
    {
        _ = streamGroupProfileId;
        string json = streamGroupService.GetStreamGroupLineupStatus();
        return Task.FromResult<IActionResult>(new ContentResult
        {
            Content = json,
            ContentType = "text/json",
            StatusCode = 200
        });
    }

    [Authorize(Policy = "SGLinks")]
    [HttpGet]
    [Route("{streamGroupProfileId}/epg.xml")]
    [Route("{streamGroupProfileId}.xml")]
    public async Task<IActionResult> GetStreamGroupEPG(int streamGroupProfileId)
    {
        if (!featureFlagService.IsFeatureEnabled(FeatureFlags.ShortLinks))
        {
            return StatusCode(403, $"{nameof(FeatureFlags.ShortLinks)} are not enabled.");
        }

        string xml = await Sender.Send(new GetStreamGroupEPG(streamGroupProfileId)).ConfigureAwait(false);
        return new FileContentResult(Encoding.UTF8.GetBytes(xml), "application/xml")
        {
            FileDownloadName = $"epg-{streamGroupProfileId}-{streamGroupProfileId}.xml"
        };
    }

    [Authorize(Policy = "SGLinks")]
    [HttpGet]
    [Route("{streamGroupProfileId}/m3u.m3u")]
    [Route("{streamGroupProfileId}.m3u")]
    public async Task<IActionResult> GetStreamGroupM3U(int streamGroupProfileId)
    {
        if (!featureFlagService.IsFeatureEnabled(FeatureFlags.ShortLinks))
        {
            return StatusCode(403, $"{nameof(FeatureFlags.ShortLinks)} are not enabled.");
        }

        string data = await Sender.Send(new GetStreamGroupM3U(streamGroupProfileId, true)).ConfigureAwait(false);

        return new FileContentResult(Encoding.UTF8.GetBytes(data), "application/x-mpegURL")
        {
            FileDownloadName = $"m3u-{streamGroupProfileId}-{streamGroupProfileId}.m3u"
        };
    }

    [Authorize(Policy = "SGLinks")]
    [HttpGet]
    [Route("{streamGroupProfileId}/auto/v{channelNumber}")]
    public async Task<IActionResult> GetAutoStream(int streamGroupProfileId, string channelNumber)
    {
        StreamGroup? streamGroup = await streamGroupService.GetStreamGroupFromSGProfileIdAsync(streamGroupProfileId).ConfigureAwait(false);
        if (streamGroup == null)
        {
            return new NotFoundResult();
        }

        (List<VideoStreamConfig> videoStreamConfigs, StreamGroupProfile streamGroupProfile) = await streamGroupService.GetStreamGroupVideoConfigsAsync(streamGroupProfileId);

        if (videoStreamConfigs is null || streamGroupProfile is null)
        {
            return new NotFoundResult();
        }
        VideoStreamConfig? videoStreamConfig = videoStreamConfigs.FirstOrDefault(a => a.ChannelNumber.ToString() == channelNumber);

        if (videoStreamConfig == null)
        {
            return new NotFoundResult();
        }

        string url = HttpContext.Request.GetUrl();
        string? encodedString = streamGroupService.EncodeStreamGroupIdProfileIdChannelId(streamGroup, streamGroupProfileId, videoStreamConfig.Id);
        string cleanName = videoStreamConfig.Name.ToCleanFileString();
        string videoUrl = $"{url}/api/videostreams/stream/{encodedString}/{cleanName}";

        return Redirect(videoUrl);
    }
}