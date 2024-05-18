using Apps.GoogleDrive.Dtos;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public record GetAllItemsResponse(List<ItemsDetailsDto> Items);