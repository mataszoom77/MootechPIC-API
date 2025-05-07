namespace MootechPic.API.Mappings
{
    using AutoMapper;
    using MootechPic.API.Models;
    using MootechPic.API.DTOs.Products;
    using MootechPic.API.DTOs.Requests;
    using MootechPic.API.DTOs.Categories;
    using MootechPic.API.DTOs.Users;
    using MootechPic.API.DTOs.Carts;
    using MootechPic.API.DTOs.SpareParts;
    using MootechPic.API.DTOs.CartItems;
    using MootechPic.API.Data;
    using MootechPic.API.Profiles.Resolvers;
    using MootechPic.API.DTOs.Wishlist;
    using MootechPic.API.DTOs.Orders.Order;
    using MootechPic.API.DTOs.Orders.OrderItem;
    using MootechPic.API.DTOs.AdminResponse;

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Output DTOs
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.Url)));

            CreateMap<SparePart, SparePartDto>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.SparePartImages.Select(spi => spi.Url)));


            CreateMap<Category, CategoryDto>();
            CreateMap<User, UserDto>();
            CreateMap<UpdateProfileDto, User>();
            CreateMap<Request, RequestDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<WishlistItemDetailsResolver>())
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom<WishlistItemImageResolver>());

            CreateMap<CreateWishlistItemDto, WishlistItem>();

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemDto>()
                .AfterMap<CartItemDetailsResolver>();

            CreateMap<ICollection<CartItem>, List<CartItemDto>>()
                .ConvertUsing((src, dest, context) => src.Select(ci => context.Mapper.Map<CartItemDto>(ci)).ToList());

            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.LineTotal,
                    opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.LineTotal,
                           opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Subtotal,
                           opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)))
                .ForMember(dest => dest.Taxes,
                           opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice) * 0.10m))
                .ForMember(dest => dest.Total,
                           opt => opt.MapFrom(src =>
                               (src.Items.Sum(i => i.Quantity * i.UnitPrice) * 1.10m)
                               + src.DeliveryCost))
                .ForMember(dest => dest.OrderItems,
                           opt => opt.MapFrom(src => src.Items));

            // CreateRequest → Request
            CreateMap<CreateRequestDto, Request>()
                .ForMember(r => r.Images, opt => opt.Ignore());

            // Request → RequestDto (expose URLs to client)
            CreateMap<Request, RequestDto>()
                .ForMember(dto => dto.ImageUrls,
                           opt => opt.MapFrom(r => r.Images.Select(i => i.Url)));

            CreateMap<AdminResponse, AdminResponseDto>();
            CreateMap<AdminResponseAttachment, AdminResponseAttachmentDto>();

            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));


            // Input DTOs -> Entities
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            CreateMap<CreateSparePartDto, SparePart>();
            CreateMap<UpdateSparePartDto, SparePart>();

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<CreateRequestDto, Request>();
            CreateMap<UpdateRequestDto, Request>();

            CreateMap<CreateCartItemDto, CartItem>();
            CreateMap<UpdateCartItemDto, CartItem>()
                .ForMember(dest => dest.Quantity,
               opt => opt.MapFrom(src => src.Quantity));
        }
    }
}
