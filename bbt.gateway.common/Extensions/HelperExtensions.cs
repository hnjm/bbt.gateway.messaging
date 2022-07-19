﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.common.Extensions
{
    public static class HelperExtensions
    {
        public static void MatchAndMap<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : class, new()
            where TDestination : class, new()
        {
            if (source != null && destination != null)
            {
                List<PropertyInfo> sourceProperties = source.GetType().GetProperties().ToList<PropertyInfo>();
                List<PropertyInfo> destinationProperties = destination.GetType().GetProperties().ToList<PropertyInfo>();

                foreach (PropertyInfo sourceProperty in sourceProperties)
                {
                    PropertyInfo destinationProperty = destinationProperties.Find(item => item.Name == sourceProperty.Name);

                    if (destinationProperty != null)
                    {
                        try
                        {
                            destinationProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

        }

        public static TDestination MapTo<TDestination>(this object source)
            where TDestination : class, new()
        {
            var destination = Activator.CreateInstance<TDestination>();
            MatchAndMap(source, destination);

            return destination;
        }

        public static List<TDestination> ListMapTo<TSource,TDestination>(this List<TSource> source)
            where TDestination : class, new()
            where TSource : class,new()
        {
            if (source == null)
                return null;

            var returnList = Activator.CreateInstance<List<TDestination>>();
            
            var destination = Activator.CreateInstance<TDestination>();
            foreach (var item in source)
            {
                MatchAndMap(item, destination);
                returnList.Add(destination);
            }
            

            return returnList;
        }


    }
}
