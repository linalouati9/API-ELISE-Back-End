using api_elise.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace api_elise.Data
{
    public class SeedData
    {
        private readonly ApplicationDbContext dataContext; 

        // Constructor
        public SeedData(ApplicationDbContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public void SeedDataContext() { 

            // Look for any models.
            if (dataContext.Models.Any())
            {
                return;   // DB has been seeded
            }

            var models = new List<Model>
            {
                new Model
                {
                    Title = "Premier modèle",
                    Description = "Description du premier modèle",
                    QRCodes = new List<QRCode>
                    {
                        new QRCode
                        {
                            Title = "QR Code 1",
                            Xslt = "<xsl:stylesheet>...</xsl:stylesheet>"
                        },
                        new QRCode
                        {
                            Title = "QR Code 2",
                            Xslt = "<xsl:stylesheet>...</xsl:stylesheet>"
                        }

                    }
                },
                new Model
                {
                    Title = "Deuxième modèle",
                    Description = "Description du deuxième modèle",
                    QRCodes = new List<QRCode>
                    {
                        new QRCode
                        {
                            Title = "QR Code 3",
                            Xslt = "<xsl:stylesheet>...</xsl:stylesheet>"
                        }
                    }
                }
            };

            dataContext.Models.AddRange(models);
            dataContext.SaveChanges();
            }
        }
    }

