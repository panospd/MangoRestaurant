﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCart.Models
{
    public class CartDetails
    {
        public int CartDetailsId { get; set; }

        public int CartHeaderId { get; set; }
        public virtual CartHeader CartHeader { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Count { get; set; }
    }
}
