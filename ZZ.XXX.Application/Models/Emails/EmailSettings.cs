﻿namespace ZZ.XXX.Application.Models.Emails
{
  public class EmailSettings
  {
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = nameof(ZZ.XXX);
  }
}
