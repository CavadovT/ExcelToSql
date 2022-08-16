using FluentValidation;
using System;

namespace ExcelToSql.DTOs
{
    public class SendFilter
    {
        public DateTime StartData { get; set; }
        public DateTime EndData { get; set; }
        public string AcceptorEmail { get; set; }
    }
    public class SendDataDtoValidator : AbstractValidator<SendFilter> 
    {
        public SendDataDtoValidator()
        {
            RuleFor(s => s.StartData).NotEmpty().WithMessage("can't be empty");
            RuleFor(s => s.EndData).NotEmpty().WithMessage("can't be empty");
            RuleFor(s => s.AcceptorEmail).NotEmpty().WithMessage("Email adress is required").EmailAddress().WithMessage("A valid email is required");
            RuleFor(c => c).Custom((c, context) =>
            {
                string [] arr = c.AcceptorEmail.Split('@');
                if (arr[1].ToLower().Trim() != "code.edu.az") 
                { 
                    context.AddFailure("AcceptorEmail", "only domain name code.edu.az");
                }
            });
            RuleFor(c => c).Custom((c, context) =>
            {
                double time = (c.EndData - c.StartData).TotalMilliseconds;
                if (time<0)
                {
                    context.AddFailure("EndData", "wrong date");
                }
            });
        }
    
    }

    public enum SendType
        {
        Segment =1,
        Country,
        Sales,
        Discounts,

        }
}
